using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetMigrations.Core
{
    public class CommandHelpWriter
    {
        private static bool IsRequired(PropertyInfo property)
        {
            bool required = property.GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Count() > 0;
            return required;
        }

        public void WriteOptionSyntax(Type argumentsType, TextWriter writer)
        {
            Dictionary<PropertyInfo, ArgumentAttribute> properties = CommandArguments.GetArgumentProperties(argumentsType);

            int count = 0;
            foreach (var prop in properties)
            {
                bool required = IsRequired(prop.Key);
                if (!required)
                {
                    writer.Write("[");
                }

                writer.Write("-");
                writer.Write(prop.Value.ShortName);
                writer.Write(" ");
                writer.Write(prop.Value.ValueName ?? prop.Value.Name);

                if (!required)
                {
                    writer.Write("]");
                }

                //  if not the last one, add a space
                if (count < properties.Count - 1)
                {
                    writer.Write(" ");
                }

                count++;
            }
        }

        public void WriteOptionList(Type argumentsType, TextWriter writer)
        {
            writer.WriteLine("Options:");

            Dictionary<PropertyInfo, ArgumentAttribute> properties = CommandArguments.GetArgumentProperties(argumentsType);

            foreach (var prop in properties)
            {
                writer.Write("\t");
                writer.Write("-");
                writer.Write(prop.Value.ShortName);
                writer.Write(", ");
                writer.Write("-");
                writer.Write(prop.Value.Name);
                writer.Write("\t\t");
                writer.WriteLine(prop.Value.Description);
            }

            //  get all props with [ArgumentAttribute]
            //  foreach, write line with name and description (if available)

            //  Usage:
            //  db.exe [-help] COMMAND [ARGS]

            //  |> db.exe -help migrate
            //  Usage:
            //  db.exe migrate -

            //  Options:
            //      -name      description
        }

        public void WriteCommandHelp(ICommand command, TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine("db.exe ");
            writer.Write(command.CommandName);
            writer.Write(" ");
            WriteOptionSyntax(command.GetArgumentsType(), writer);
        }
    }
}