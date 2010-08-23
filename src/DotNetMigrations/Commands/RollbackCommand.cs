using System;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    public class RollbackCommand : DatabaseCommandBase<DatabaseCommandArguments>
    {
        private readonly DatabaseCommandBase<MigrateCommandArgs> _migrateCommand;

        public RollbackCommand()
            : this(new MigrateCommand())
        {
        }

        public RollbackCommand(DatabaseCommandBase<MigrateCommandArgs> migrateCommand)
        {
            _migrateCommand = migrateCommand;
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
        protected override void Run(DatabaseCommandArguments args)
        {
            long currentVersion = GetDatabaseVersion();
            long previousVersion = GetPreviousDatabaseVersion(currentVersion);

            if (previousVersion == -1)
            {
                Log.WriteLine("No rollback is necessary. Database schema is already at version 0.");
                return;
            }

            _migrateCommand.Log = Log;

            var migrateCommandArgs = new MigrateCommandArgs();
            migrateCommandArgs.Connection = args.Connection;
            migrateCommandArgs.TargetVersion = previousVersion;

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