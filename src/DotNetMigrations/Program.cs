using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;

namespace DotNetMigrations
{
    internal class Program
    {
        #region Void Main

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }

        #endregion

        private readonly CommandRepository _commandRepo;
        private readonly LogRepository _logger;
        private readonly bool _logFullErrors;

        private Program() : this(new ConfigurationManagerWrapper())
        {
        }

        /// <summary>
        /// Program constructor, instantiates primary repository objects.
        /// </summary>
        private Program(IConfigurationManager configManager)
        {
            _commandRepo = new CommandRepository();
            _logger = new LogRepository();

            string logFullErrorsSetting = configManager.AppSettings["logFullErrors"];
            bool.TryParse(logFullErrorsSetting, out _logFullErrors);
        }

        /// <summary>
        /// Primary Program Execution
        /// </summary>
        private void Run(string[] args)
        {
            string executableName = Process.GetCurrentProcess().ProcessName + ".exe";
            ArgumentSet allArguments = ArgumentSet.Parse(args);

            var helpWriter = new CommandHelpWriter(_logger);

            bool showHelp = allArguments.ContainsName("help");

            string commandName = showHelp
                                     ? allArguments.GetByName("help")
                                     : allArguments.AnonymousArgs.FirstOrDefault();

            ICommand command = null;

            if (commandName != null)
            {
                command = _commandRepo.GetCommand(commandName);
            }
            
            if (command == null)
            {
                if (showHelp)
                {
                    //  no command name was found, show the list of available commands
                    WriteAppUsageHelp(executableName);
                    helpWriter.WriteCommandList(_commandRepo.Commands);
                }
                else
                {
                    //  invalid command name was given
                    _logger.WriteError("'{0}' is not a DotNetMigrations command.", commandName);
                    _logger.WriteLine(string.Empty);
                    _logger.WriteError("See '{0} -help' for a list of available commands.", executableName);
                }
            }

            if (showHelp && command != null)
            {
                //  show help for the given command
                helpWriter.WriteCommandHelp(command, executableName);
            }
            else if (command != null)
            {
                command.Log = _logger;

                var commandArgumentSet = ArgumentSet.Parse(args.Skip(1).ToArray());
                IArguments commandArgs = command.CreateArguments();
                commandArgs.Parse(commandArgumentSet);

                if (commandArgs.IsValid)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    try
                    {
                        command.Run(commandArgs);
                    }
                    catch (Exception ex)
                    {
                        //_logger.WriteLine(string.Empty);

                        if (_logFullErrors)
                        {
                            _logger.WriteError(ex.ToString());
                        }
                        else
                        {
                            WriteShortErrorMessages(ex);
                        }

                        if (Debugger.IsAttached)
                            throw;
                    }
                    finally
                    {
                        timer.Stop();

                        _logger.WriteLine(string.Empty);
                        _logger.WriteLine(string.Format("Command duration was {0}.",
                                                               decimal.Divide(timer.ElapsedMilliseconds, 1000).ToString(
                                                                   "0.0000s")));
                    }
                }
                else
                {
                    //  argument validation failed, show errors
                    WriteValidationErrors(command.CommandName, commandArgs.Errors);
                    _logger.WriteLine(string.Empty);
                    helpWriter.WriteCommandHelp(command, executableName);
                }
            }
        }

        private void WriteShortErrorMessages(Exception ex)
        {
            _logger.WriteLine(ex.Message);
            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                _logger.WriteLine(string.Empty);
                _logger.WriteLine(innerEx.Message);

                innerEx = innerEx.InnerException;
            }
        }

        /// <summary>
        /// Writes usage help for the app to the logger.
        /// </summary>
        private void WriteAppUsageHelp(string executableName)
        {
            //_logger.WriteLine(string.Empty);
            _logger.Write("Usage: ");
            _logger.Write(executableName);
            _logger.WriteLine(" [-help] command [args]");
        }

        /// <summary>
        /// Writes out all validation errors to the logger.
        /// </summary>
        private void WriteValidationErrors(string commandName, IEnumerable<string> errors)
        {
            _logger.WriteError("Invalid arguments for the {0} command", commandName);
            _logger.WriteLine(string.Empty);
            errors.ForEach(x => _logger.WriteError("\t* " + x));
        }
    }
}