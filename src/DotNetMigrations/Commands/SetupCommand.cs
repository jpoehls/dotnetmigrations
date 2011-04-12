using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public class SetupCommand : DatabaseCommandBase<SeedCommandArgs>
    {
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

        protected override void Run(SeedCommandArgs args)
        {
            // migrate the schema first
            var migrateCmd = new MigrateCommand();
            var migrateCmdArgs = new MigrateCommandArgs() {Connection = args.Connection};

            migrateCmd.Run(migrateCmdArgs);

            // run the seed scripts
            var seedCmd = new SeedCommand();
            seedCmd.Run(args);
        }
    }
}