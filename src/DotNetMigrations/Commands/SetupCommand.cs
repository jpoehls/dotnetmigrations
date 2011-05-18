using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public class SetupCommand : DatabaseCommandBase<SeedCommandArgs>
    {
        private readonly DatabaseCommandBase<MigrateCommandArgs> _migrateCommand;
        private readonly DatabaseCommandBase<SeedCommandArgs> _seedCommand;

        public override string CommandName
        {
            get { return "setup"; }
        }

        public override string Description
        {
            get
            {
                return "Sets up a new database by migrating to the latest version"
                       + " and executing the seed scripts.";
            }
        }

        public SetupCommand()
            : this(new MigrateCommand(), new SeedCommand())
        {
        }

        public SetupCommand(DatabaseCommandBase<MigrateCommandArgs> migrateCommand,
            DatabaseCommandBase<SeedCommandArgs> seedCommand)
        {
            _migrateCommand = migrateCommand;
            _seedCommand = seedCommand;
        }

        protected override void Execute(SeedCommandArgs args)
        {
            // migrate the schema first
            _migrateCommand.Log = this.Log;
            var migrateCmdArgs = new MigrateCommandArgs() { Connection = args.Connection };

            _migrateCommand.Run(migrateCmdArgs);

            // run the seed scripts
            _seedCommand.Log = this.Log;
            _seedCommand.Run(args);
        }
    }
}