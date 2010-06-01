using System;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    internal class GenerateScriptCommand : CommandBase
    {
        private readonly IMigrationDirectory _migrationDirectory;

        public GenerateScriptCommand() : this(new MigrationDirectory())
        {
        }

        public GenerateScriptCommand(IMigrationDirectory migrationDirectory)
        {
            _migrationDirectory = migrationDirectory;
        }

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
        /// Creates the .sql file and sends the final message.
        /// </summary>
        protected override void RunCommand()
        {
            string migrationName = Arguments.GetArgument(1);
            string path = _migrationDirectory.CreateBlankScript(migrationName);

            Log.WriteLine("The new migration script, " + Path.GetFileName(path) + ", was created successfully!");
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
    }
}