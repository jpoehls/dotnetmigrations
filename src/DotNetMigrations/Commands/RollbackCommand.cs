using System;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    public class RollbackCommand : DatabaseCommandBase<DatabaseCommandArguments>
    {
        private readonly DatabaseCommandBase<MigrateCommandArgs> _migrateCommand;
		private readonly IMigrationDirectory _migrationDirectory;

        public RollbackCommand()
            : this(new MigrateCommand(), new MigrationDirectory())
        {
        }

		public RollbackCommand(DatabaseCommandBase<MigrateCommandArgs> migrateCommand, IMigrationDirectory migrationDirectory)
        {
            _migrateCommand = migrateCommand;
			_migrationDirectory = migrationDirectory;
        }

        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return "rollback"; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string Description
        {
            get { return "Rolls back the database by one version."; }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void Execute(DatabaseCommandArguments args)
        {
            long currentVersion = GetDatabaseVersion();

			if(currentVersion == 0)
			{
				Log.WriteLine("No rollback is necessary. Database schema is already at version 0.");
				return;
			}

			var previousVersion = _migrationDirectory.GetScripts()
				.OrderBy(x => x.Version)
				.Where(x => x.Version < currentVersion)
				.LastOrDefault();

			if(previousVersion == null)
			{
				Log.WriteLine("No rollback is necessary. No earlier migration to run.");
				return;
			}

            _migrateCommand.Log = Log;

            var migrateCommandArgs = new MigrateCommandArgs();
            migrateCommandArgs.Connection = args.Connection;
            migrateCommandArgs.TargetVersion = previousVersion.Version;

            _migrateCommand.Run(migrateCommandArgs);
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <returns>The current version of the database.</returns>
        private long GetPreviousDatabaseVersion(long currentVersion)
        {
            string cmdText = string.Format("SELECT MAX([version]) FROM [schema_migrations] WHERE [version] <> {0}",
                                           currentVersion);

            long previousVersion;

            using (DbCommand cmd = Database.CreateCommand())
            {
                cmd.CommandText = cmdText;
                previousVersion = cmd.ExecuteScalar<long>(-1);
            }

            return previousVersion;
        }
    }
}