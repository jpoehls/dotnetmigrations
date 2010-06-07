using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
            bool optional = property.GetCustomAttributes(typeof (RequiredAttribute), false)
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

            int count = 0;
            foreach (var prop in properties)
            {
                bool optional = IsOptional(prop.Key);
                if (optional)
                {
                    _log.Write("[");
                }

                _log.Write("-");
                _log.Write(prop.Value.ShortName);
                _log.Write(" ");
                _log.Write(prop.Value.ValueName ?? prop.Value.Name);

                if (optional)
                {
                    _log.Write("]");
                }

                //  if not the last one, add a space
                if (count < properties.Count - 1)
                {
                    _log.Write(" ");
                }

                count++;
            }
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

            foreach (var prop in properties)
            {
                _log.Write("  -");
                _log.Write(prop.Value.ShortName);
                _log.Write(", ");
                _log.Write("-");
                _log.Write(prop.Value.Name);
                _log.Write("\t\t");
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

            _log.WriteLine(string.Empty);

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

            foreach (ICommand cmd in commands)
            {
                _log.WriteLine("  {0}\t\t{1}", cmd.CommandName, cmd.Description);
            }
        }
    }
}