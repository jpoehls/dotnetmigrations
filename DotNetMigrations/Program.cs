using System;
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

        private readonly CommandRepository _cmdRepository;
        private readonly LogRepository _logRepository;

        /// <summary>
        /// Program constructor, instantiates primary repository objects.
        /// </summary>
        private Program()
        {
            _cmdRepository = new CommandRepository();
            _logRepository = new LogRepository();
        }

        /// <summary>
        /// Primary Program Execution
        /// </summary>
        private void Run(string[] args)
        {
            ArgumentSet set = ArgumentSet.Parse(args);

            var helpWriter = new CommandHelpWriter(_logRepository);

            bool showHelp = set.NamedArgs.ContainsKey("help");

            string commandName = showHelp
                                     ? set.NamedArgs["help"]
                                     : set.AnonymousArgs.FirstOrDefault();

            ICommand command = null;

            if (commandName != null)
            {
                command = _cmdRepository.GetCommand(commandName);
            }
            else
            {
                WriteCommandList();
            }

            if (showHelp && command != null)
            {
                helpWriter.WriteCommandHelp(command, "db.exe");
            }
            else if (command != null)
            {
                command.Log = _logRepository;

                var commandArgs = command.CreateArguments();
                commandArgs.Parse(set);

                if (commandArgs.IsValid)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    command.Run(commandArgs);

                    timer.Stop();
                    _logRepository.WriteLine(string.Format("Process Finished in: {0}.",
                                                           decimal.Divide(timer.ElapsedMilliseconds, 1000).ToString(
                                                               "0.0000s")));
                }
                else
                {
                    //  ALSO WRITE ERROR MESSAGES OUT
                    helpWriter.WriteArgumentSyntax(command.GetArgumentsType());
                }
            }

            //new HelpCommand(_logRepository).ShowHelp(cmd);
        }

        private void WriteCommandList()
        {
            foreach (var cmd in _cmdRepository.Commands)
            {
                _logRepository.WriteLine(
                    "\t" +
                    cmd.CommandName +
                    "\t\t" +
                    cmd.Description);
            }
        }
    }
}