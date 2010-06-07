using System;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class RollbackCommand : DatabaseCommandBase<DatabaseCommandArguments>
    {
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
        protected override void Run(DatabaseCommandArguments args)
        {
            long currentVersion = GetDatabaseVersion();
            long previousVersion = GetPreviousDatabaseVersion(currentVersion);

            var migrationCommand = new MigrateCommand();
            migrationCommand.Log = Log;

            var migrateCommandArgs = new MigrateCommandArgs();
            migrateCommandArgs.Connection = args.Connection;
            migrateCommandArgs.TargetVersion = previousVersion;

            migrationCommand.Run(migrateCommandArgs);
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <returns>The current version of the database.</returns>
        private long GetPreviousDatabaseVersion(long currentVersion)
        {
            string cmdText = string.Format("SELECT MAX([version]) FROM [schema_migrations] WHERE [version] <> {0}",
                                           currentVersion);

            long previousVersion = -1;

            try
            {
                using (DbCommand cmd = Database.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    var version = cmd.ExecuteScalar<string>();
                    if (version != null)
                    {
                        version = version.Trim();
                    }
                    long.TryParse(version, out previousVersion);
                }
            }
            catch (DbException ex)
            {
                Log.WriteError(ex.Message);
            }

            return previousVersion;
        }
    }
}