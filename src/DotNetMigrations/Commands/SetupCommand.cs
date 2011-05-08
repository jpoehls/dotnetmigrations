using System.ComponentModel;
using DotConsole;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    [Command("setup")]
    [Description("Sets up a new database by migrating to the latest version"
               + " and executing the seed scripts.")]
    public class SetupCommand : DatabaseCommandBase
    {
        private readonly DatabaseCommandBase _migrateCommand;
        private readonly DatabaseCommandBase _seedCommand;

        [Parameter("set", Flag = 's', Position = 1)]
        [Description("Set of seed data to plant.")]
        public string SetName { get; set; }

        public SetupCommand()
            : this(new MigrateCommand(), new SeedCommand())
        {
        }

        public SetupCommand(DatabaseCommandBase migrateCommand,
            DatabaseCommandBase seedCommand)
        {
            _migrateCommand = migrateCommand;
            _seedCommand = seedCommand;
        }

        public override void Execute()
        {
            // migrate the schema first
            _migrateCommand.Log = this.Log;
            _migrateCommand.Connection = Connection;
            _migrateCommand.Run();

            // run the seed scripts
            _seedCommand.Log = this.Log;
            _seedCommand.Connection = Connection;
            _seedCommand.Run();
        }
    }
}