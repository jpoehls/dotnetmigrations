using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DotConsole;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
using ICommand = DotNetMigrations.Core.ICommand;

namespace DotNetMigrations
{
    internal class Program
    {
        private static Program _current = null;
        public static Program Current
        {
            get
            {
                if (_current == null)
                    _current = new Program();
                return _current;
            }
        }

        #region Void Main

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Current.Run(args);
        }

        #endregion

        private readonly LogRepository _logger;
        private readonly bool _logFullErrors;
        private readonly Commander _commander;

        private readonly bool _keepConsoleOpen;

        public ICommandLocator CommandLocator { get { return _commander.Router.Locator; } }

        private Program()
            : this(new ConfigurationManagerWrapper())
        {
        }

        /// <summary>
        /// Program constructor, instantiates primary repository objects.
        /// </summary>
        private Program(IConfigurationManager configManager)
        {
            _logger = new LogRepository();

            var pluginDirectory = configManager.AppSettings[AppSettingKeys.PluginFolder];
            
            _commander = Commander.Standard(new AssemblyCatalog(Assembly.GetCallingAssembly()), new DirectoryCatalog(pluginDirectory));
           
            string logFullErrorsSetting = configManager.AppSettings[AppSettingKeys.LogFullErrors];
            bool.TryParse(logFullErrorsSetting, out _logFullErrors);

            _keepConsoleOpen = ProgramLaunchedInSeparateConsoleWindow();
        }

        /// <summary>
        /// Primary Program Execution
        /// </summary>
        private void Run(IEnumerable<string> args)
        {
            _commander.Run(args);

            if (_keepConsoleOpen)
            {
                string executableName = Process.GetCurrentProcess().ProcessName + ".exe";

                Console.WriteLine();
                Console.WriteLine("  > Uh-oh. It looks like you didn't run me from a console.");
                Console.WriteLine("  > Did you double-click me?");
                Console.WriteLine("  > I don't like to be clicked.");
                Console.WriteLine("  > Please open a command prompt and run me by typing " + executableName + ".");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.Read();
            }
        }

        /// <summary>
        /// Determines if the program was launched in a separate console window
        /// or whether it was run from within an existing console.
        /// i.e. launched by click the EXE in Windows Explorer or by typing db.exe at the command line
        /// </summary>
        /// <remarks>
        /// This method isn't full proof.
        /// http://support.microsoft.com/kb/99115
        /// and
        /// http://stackoverflow.com/questions/510805/can-a-win32-console-application-detect-if-it-has-been-run-from-the-explorer-or-no
        /// </remarks>
        private static bool ProgramLaunchedInSeparateConsoleWindow()
        {
            //  if no debugger is attached
            if (!Debugger.IsAttached && TestConsole())
                //  and the cursor position appears untouched
                if (Console.CursorLeft == 0 && Console.CursorTop == 1)
                    //  and there are no arguments
                    //  (we allow for 1 arg because the first arg appears
                    //   to always be the path to the executable being run)
                    if (Environment.GetCommandLineArgs().Length <= 1)
                        //  then assume we were launched into a separate console window
                        return true;

            //  looks like we were launched from command line, good!
            return false;
        }

        /// <summary>
        /// Returns true/false whether a console window is available.
        /// </summary>
        private static bool TestConsole()
        {
            try
            {
#pragma warning disable 168
                var x = Console.CursorLeft;
#pragma warning restore 168
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}