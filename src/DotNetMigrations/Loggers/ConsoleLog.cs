using System;
using System.Linq;
using DotNetMigrations.Core;
using System.IO;

namespace DotNetMigrations.Loggers
{
    internal class ConsoleLog : LoggerBase
    {
        private string _logName = "ConsoleLog";
        private const ConsoleColor WarningColor = ConsoleColor.Yellow;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;

        public ConsoleLog()
        {
            //  pad the console with a blank line
            Console.WriteLine();
        }

        /// <summary>
        /// The name of the log.
        /// </summary>
        public override string LogName
        {
            get { return _logName; }
            set { _logName = value; }
        }

        /// <summary>
        /// Writes output to the console. Intelligently wraps
        /// text to fit the console's remaining line buffer,
        /// aligning wrapped lines to the current cursor position.
        /// </summary>
        public override void Write(string message)
        {
            //  remaining usable space on the current line
			int remainingBufferLength = 0;
			try
			{
				remainingBufferLength = Console.BufferWidth - Console.CursorLeft;
			}
			catch(IOException)
			{
				// console width and position cannot always be found (when running as part of a TeamCity build for instance)
				remainingBufferLength = message.Length;
			}

            //  if the message fits on the current line, write it
            if (message.Length <= remainingBufferLength)
            {
                Console.Write(message);
            }
            else
            {
                int alignmentPosition = Console.CursorLeft;
                int maxLineLength = remainingBufferLength;

                var words = message.Split(' ');

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Length + 1 > remainingBufferLength)
                    {
                        Console.WriteLine(string.Empty);
                        Console.SetCursorPosition(alignmentPosition, Console.CursorTop);
                        remainingBufferLength = maxLineLength;
                    }

                    Console.Write(words[i]);

                    if (i < words.Length - 1)
                    {
                        Console.Write(" ");
                    }

                    remainingBufferLength -= words[i].Length + 1;
                }
            }
        }

        /// <summary>
        /// Writes a line of text to the console window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            Write(message);
            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// Writes a line of yellow text to the console window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteWarning(string message)
        {
            ConsoleColor previous = Console.ForegroundColor;

            Console.ForegroundColor = WarningColor;
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

            Console.ForegroundColor = ErrorColor;
            WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}