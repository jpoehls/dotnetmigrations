using System;
using System.Collections.Generic;
using System.Text;
using DotNetMigrations.Core;
using DotNetMigrations.Logs;

namespace DotNetMigrations.Commands.Special
{
    internal class HelpCommand
    {
        private readonly ILogger log;

        /// <summary>
        /// Instantiates a new instance of the HelpCommand class.
        /// </summary>
        /// <remarks>Sets the default log to an instance of the ConsoleLog class.</remarks>
        internal HelpCommand()
            : this(new ConsoleLog())
        {
        }

        /// <summary>
        /// Instantiates a new instance of the HelpCommand class.
        /// </summary>
        /// <param name="logger">An instance of a class that implements the ILogger interface.</param>
        internal HelpCommand(ILogger logger)
        {
            log = logger;
        }

        /// <summary>
        /// Displays Help Text specific command
        /// </summary>
        /// <param name="command">The command that seeks specific help text.</param>
        internal void ShowHelp(ICommand command)
        {
            log.WriteLine(FormatHelpText(command));
            log.WriteLine(string.Empty);
        }

        /// <summary>
        /// Displays Help Text for all commands provided.
        /// </summary>
        /// <param name="commands">The command collection to display information about.</param>
        internal void ShowHelp(IEnumerable<ICommand> commands)
        {
            foreach (ICommand command in commands)
            {
                log.WriteLine(FormatHelpText(command));
                log.WriteLine(string.Empty);
            }
        }

        /// <summary>
        /// Formats the help text for the command.
        /// </summary>
        /// <param name="command">The command whose help text will be used.</param>
        /// <returns>A formatted strign.</returns>
        private static string FormatHelpText(ICommand command)
        {
            var sb = new StringBuilder();
            sb.AppendLine(command.CommandName);
            sb.AppendLine(string.Empty); //empty line

            List<string> splitText = SplitText(command.Description);

            splitText.ForEach(t => sb.AppendLine(t));

            return sb.ToString();
        }

        /// <summary>
        /// Splits the text into an appropriate length.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <returns>A list of the split text.</returns>
        private static List<string> SplitText(string text)
        {
            const int LINE_LENGTH = 55;
            int length = text.Length;
            var output = new List<string>();

            for (int i = 0; i*LINE_LENGTH < length; i++)
            {
                if (i*LINE_LENGTH + LINE_LENGTH <= length)
                {
                    output.Add(text.Substring(i*LINE_LENGTH, LINE_LENGTH).Trim().PadRight(55, ' ').PadLeft(60, ' '));
                }
                else
                {
                    output.Add(text.Substring(i*LINE_LENGTH).Trim().PadRight(55, ' ').PadLeft(60, ' '));
                }
            }

            return output;
        }
    }
}