using System.Collections.Generic;
using System.Diagnostics;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Repositories;
using DotNetMigrations.Core;

namespace DotNetMigrations
{
    class Program
    {
        #region Void Main

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Program p = new Program(args);
            p.Run();
        }

        #endregion
        
        private ArgumentRepository _argRepository;
        private CommandRepository _cmdRepository;
        private LogRepository _logRepository;

        /// <summary>
        /// Program constructor, instantiates primary repository objects.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private Program(IEnumerable<string> args)
        {
            _argRepository = new ArgumentRepository(args);
            _cmdRepository = new CommandRepository();
            _logRepository = new LogRepository();
        }

        /// <summary>
        /// Primary Program Execution
        /// </summary>
        private void Run()
        {
            if(CheckForFirstArgument())
            {
                return;
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();

            var cmd = GetCommand();

            cmd.Log = _logRepository;
            cmd.Arguments = _argRepository;

            CommandResults results = cmd.Run();

            if (results == CommandResults.Invalid)
            {
                new HelpCommand(_logRepository).ShowHelp(cmd);
            }

            timer.Stop();

            _logRepository.WriteLine(string.Format("Process Finished in: {0}.", decimal.Divide(timer.ElapsedMilliseconds, 1000).ToString("0.0000s")));
        }

        /// <summary>
        /// Shows the help screen if there are no arguments or if the first isn't a command.
        /// </summary>
        private bool CheckForFirstArgument()
        {
            bool invalid = false;
            
            invalid = (_argRepository.Arguments.Count == 0);
            invalid = (invalid || _cmdRepository.GetCommand(_argRepository.GetArgument(0)) == null);

            if (invalid)
            {
                var helpcmd = new HelpCommand(_logRepository);
                helpcmd.ShowHelp(_cmdRepository.Commands);
            }

            return (invalid);
        }

        /// <summary>
        /// Iterates through commands until it finds an argument that
        /// is not a command or runs out of arguments.
        /// </summary>
        /// <returns>An instance of command located from the interations.</returns>
        private ICommand GetCommand()
        {
            ICommand cmd = _cmdRepository.GetCommand(_argRepository.GetArgument(0));
            return cmd;
        }
    }
}
