using System;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using DotConsole;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    [Command("rollback")]
    [Description("Rolls back the database by one version.")]
    public class RollbackCommand : DatabaseCommandBase
    {
        private readonly MigrateCommand _migrateCommand;

        public RollbackCommand()
            : this(new MigrateCommand())
        {
        }

        public RollbackCommand(MigrateCommand migrateCommand)
        {
            _migrateCommand = migrateCommand;
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        public override void Execute()
        {
            long currentVersion = GetDatabaseVersion();
            long previousVersion = GetPreviousDatabaseVersion(currentVersion);

            if (previousVersion == -1)
            {
                Log.WriteLine("No rollback is necessary. Database schema is already at version 0.");
                return;
            }

            _migrateCommand.Log = Log;
            _migrateCommand.Connection = Connection;
            _migrateCommand.TargetVersion = previousVersion;

            _migrateCommand.Run();
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