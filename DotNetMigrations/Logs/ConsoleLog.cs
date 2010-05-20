using System;
using DotNetMigrations.Core;

namespace DotNetMigrations.Logs
{
    internal class ConsoleLog : LoggerBase
    {
        private string _logName = "ConsoleLog";

        /// <summary>
        /// The name of the log.
        /// </summary>
        public override string LogName
        {
            get { return _logName; }
            set { _logName = value; }
        }

        /// <summary>
        /// Writes a line of text to the console window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
             Console.WriteLine(message);
        }

        /// <summary>
        /// Writes a line of yellow text to the console window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteWarning(string message)
        {
            ConsoleColor previous = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine(message);
            Console.ForegroundColor = previous;
        }

        /// <summary>
        /// Writes a line of red text to the console window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteError(string message)
        {
            ConsoleColor previous = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}
