using System;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    internal class GenerateScriptCommand : CommandBase<GenerateScriptCommandArgs>
    {
        private readonly IMigrationDirectory _migrationDirectory;

        public GenerateScriptCommand()
            : this(new MigrationDirectory())
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
        protected override void Run(GenerateScriptCommandArgs args)
        {
            string path = _migrationDirectory.CreateBlankScript(args.MigrationName);

            Log.WriteLine("The new migration script, " + Path.GetFileName(path) + ", was created successfully!");
        }
    }
}