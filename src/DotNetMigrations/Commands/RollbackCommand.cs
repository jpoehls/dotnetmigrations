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

			var allscripts = _migrationDirectory.GetScripts()
				.OrderBy(x => x.Version);

			long targetversion = 0;

			// Get the first script behind the current version of the database
			var previousVersion = allscripts
				.Where(x => x.Version < currentVersion)
				.LastOrDefault();

			// If the database is at its lowest migration left, rollback the first migration
			if(currentVersion > 0 && previousVersion == null)
				targetversion = 0;
			else
				targetversion = previousVersion.Version;

            _migrateCommand.Log = Log;

            var migrateCommandArgs = new MigrateCommandArgs();
            migrateCommandArgs.Connection = args.Connection;
            migrateCommandArgs.TargetVersion = targetversion;

            _migrateCommand.Run(migrateCommandArgs);
        }

        /// <summary>
        /// Retrieves the previous schema version of the database.
        /// </summary>
        /// <returns>The current version of the database.</returns>
        private long GetPreviousDatabaseVersion(long currentVersion)
        {
            // note that we have to use 'IN' instead of '=' for the subquery
            // becuase sql server compact doesn't support subqueries that return scalar values
            string cmdText = string.Format("SELECT [version] FROM [schema_migrations] WHERE [id] IN (SELECT MAX([id]) FROM [schema_migrations] WHERE [version] <> {0})",
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