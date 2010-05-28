using System;
using System.IO;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class GenerateScriptCommand : CommandBase
    {
        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return "generate"; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string HelpText
        {
            get
            {
                return "Generates a new migration script in the migration directory."
                       + "\r\nUsage: db generate <connection_string> <migration_name>";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            string scriptPath = MigrationScriptHelper.GetScriptPath(Log);
            GenerateScript(scriptPath);
        }

        /// <summary>
        /// Validates the arguments for the command
        /// </summary>
        /// <returns>True if the arguments are valid, else false.</returns>
        protected override bool ValidateArguments()
        {
            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number of arguments for the Generate command is too few.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the .sql file and sends the final message.
        /// </summary>
        /// <param name="path">The directory to create the new script.</param>
        private void GenerateScript(string path)
        {
            string migrationName = Arguments.GetArgument(1);
            string scriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + migrationName + ".sql";

            string file = Path.Combine(path, scriptName);

            using (StreamWriter writer = File.CreateText(file))
            {
                writer.WriteLine("BEGIN_SETUP:\r\n\r\n\r\n");
                writer.WriteLine("END_SETUP:");
                writer.WriteLine("BEGIN_TEARDOWN:\r\n\r\n\r\n");
                writer.WriteLine("END_TEARDOWN:");
            }

            Log.WriteLine("The new migration script, " + scriptName + ", was created successfully!");
        }
    }
}