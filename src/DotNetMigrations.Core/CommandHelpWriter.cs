using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetMigrations.Core
{
    public class CommandHelpWriter
    {
        private readonly ILogger _log;

        public CommandHelpWriter(ILogger logger)
        {
            _log = logger;
        }

        /// <summary>
        /// Returns True/False whether the given property is optional.
        /// </summary>
        private static bool IsOptional(ICustomAttributeProvider property)
        {
            bool optional = property.GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Count() == 0;
            return optional;
        }

        /// <summary>
        /// Writes out the argument syntax for the given arguments Type.
        /// </summary>
        public void WriteArgumentSyntax(Type argumentsType)
        {
            //  SAMPLE OUTPUT
            //
            //  -requiredArg req_value_name [-optionalArg1 value_name] [-optionalArg2 value_name]

            Dictionary<PropertyInfo, ArgumentAttribute> properties =
                CommandArguments.GetArgumentProperties(argumentsType);

            var syntax = new StringBuilder();

            int count = 0;
            foreach (var prop in properties)
            {
                bool optional = IsOptional(prop.Key);
                if (optional)
                {
                    syntax.Append("[");
                }

                syntax.Append("-");
                syntax.Append(prop.Value.ShortName);
                syntax.Append(" ");
                syntax.Append(prop.Value.ValueName ?? prop.Value.Name);

                if (optional)
                {
                    syntax.Append("]");
                }

                //  if not the last one, add a space
                if (count < properties.Count - 1)
                {
                    syntax.Append(" ");
                }

                count++;
            }

            _log.Write(syntax.ToString());
        }

        /// <summary>
        /// Writes out the list of arguments for the given arguments Type.
        /// </summary>
        public void WriteArgumentList(Type argumentsType)
        {
            //  SAMPLE OUTPUT
            //
            //  Options:
            //    -f, -firstArg       description of first argument
            //    -s, -secondArg      description of second argument

            _log.WriteLine(string.Empty);
            _log.WriteLine("Options:");

            Dictionary<PropertyInfo, ArgumentAttribute> properties =
                CommandArguments.GetArgumentProperties(argumentsType);

            int maxArgNameLength = properties.Max(x => x.Value.ShortName.Length + x.Value.Name.Length) + 4;

            foreach (var prop in properties)
            {
                _log.Write("".PadLeft(IndentWidth));
                _log.Write(string.Format("-{0}, -{1}", prop.Value.ShortName, prop.Value.Name).PadRight(maxArgNameLength + TabWidth));
                _log.WriteLine(prop.Value.Description);
            }
        }

        /// <summary>
        /// Writes out the help verbiage for the given command.
        /// </summary>
        public void WriteCommandHelp(ICommand command, string executableName)
        {
            //  SAMPLE OUTPUT
            //
            //  Usage: db.exe commandName [ARGUMENT SYNTAX]
            //  
            //  Options:
            //    -f, -firstArg       description of first argument
            //    -s, -secondArg      description of second argument       

            _log.Write("Usage: ");
            _log.Write(executableName);
            _log.Write(" ");
            _log.Write(command.CommandName);
            _log.Write(" ");

            var argType = command.GetArgumentsType();
            WriteArgumentSyntax(argType);

            _log.WriteLine(string.Empty);
            WriteArgumentList(argType);
        }

        /// <summary>
        /// Writes out a list of the given command names and descriptions.
        /// </summary>
        /// <param name="commands"></param>
        public void WriteCommandList(IEnumerable<ICommand> commands)
        {
            //  SAMPLE OUTPUT
            //
            //  Commands:
            //    firstCommand        description of first command
            //    secondCommand       description of second command

            _log.WriteLine(string.Empty);
            _log.WriteLine("Available commands:");

            int maxCommandNameLength = commands.Max(x => x.CommandName.Length);

            foreach (ICommand cmd in commands)
            {
                _log.Write("".PadLeft(2));
                _log.Write("{0}", cmd.CommandName.PadRight(maxCommandNameLength + TabWidth));
                _log.WriteLine(cmd.Description);
            }
        }

        private const int IndentWidth = 2;
        private const int TabWidth = 4;
    }
}