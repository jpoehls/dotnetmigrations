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
        private bool _logFullErrors;

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
            ArgumentSet set = ArgumentSet.Parse(args);

            var helpWriter = new CommandHelpWriter(_logger);

            bool showHelp = set.NamedArgs.ContainsKey("help");

            string commandName = showHelp
                                     ? set.NamedArgs["help"]
                                     : set.AnonymousArgs.FirstOrDefault();

            ICommand command = null;

            if (commandName != null)
            {
                command = _commandRepo.GetCommand(commandName);
            }
            else
            {
                //  no command name was found, show the list of available commands
                helpWriter.WriteCommandList(_commandRepo.Commands);
            }

            if (showHelp && command != null)
            {
                //  show help for the given command
                helpWriter.WriteCommandHelp(command, executableName);
            }
            else if (command != null)
            {
                command.Log = _logger;

                IArguments commandArgs = command.CreateArguments();
                commandArgs.Parse(set);

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
                        if (_logFullErrors)
                        {
                            _logger.WriteError(ex.ToString());
                        }
                        else
                        {
                            _logger.WriteError(ex.Message);
                        }

                        if (Debugger.IsAttached)
                            throw;
                    }
                    finally
                    {
                        timer.Stop();
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