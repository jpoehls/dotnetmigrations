using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using DotConsole;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    [Command("generate")]
    [Description("Generates a new migration script in the migration directory.")]
    public class GenerateScriptCommand : CommandBase
    {
        private readonly IMigrationDirectory _migrationDirectory;

        [Required(ErrorMessage = "-name is required")]
        [Parameter("name", Flag='n', Position = 0, MetaName = "migration_name")]
        [Description("Name of the migration script to generate")]
        public string MigrationName { get; set; }

        public GenerateScriptCommand()
            : this(new MigrationDirectory())
        {
        }

        public GenerateScriptCommand(IMigrationDirectory migrationDirectory)
        {
            _migrationDirectory = migrationDirectory;
        }

        /// <summary>
        /// Creates the .sql file and sends the final message.
        /// </summary>
        public override void Execute()
        {
            string path = _migrationDirectory.CreateBlankScript(MigrationName);

            Log.WriteLine("The new migration script " + Path.GetFileName(path) + " was created successfully!");
        }
    }
}