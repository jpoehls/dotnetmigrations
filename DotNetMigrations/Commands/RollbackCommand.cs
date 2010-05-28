using System;
using System.Data.Common;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class RollbackCommand : DatabaseCommandBase
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
        public override string HelpText
        {
            get
            {
                return "Rolls back the database by one version."
                       + "\r\nExample: rollback <MigrateName> [ConnectionString]";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            base.RunCommand();

            long currentVersion = GetDatabaseVersion();
            long previousVersion = GetPreviousDatabaseVersion(currentVersion);

            Arguments.Arguments.Add(previousVersion.ToString());

            var migrationCommand = new MigrateCommand();
            migrationCommand.Arguments = Arguments;
            migrationCommand.Log = Log;

            CommandResults results = migrationCommand.Run();

            if (results != CommandResults.Success)
            {
                Log.WriteError("Rollback failed.");
            }
        }

        /// <summary>
        /// Validates the arguments for the command
        /// </summary>
        /// <returns>True if the arguments are valid, else false.</returns>
        /// <remarks>
        /// Allowed Argument Structure:
        /// db.exe rollback connectionString [version]
        /// </remarks>
        protected override bool ValidateArguments()
        {
            bool valid = base.ValidateArguments();
            if (!valid)
                return false;

            // The 1st argument is the command name.
            // The 2nd argument is the connection string.
            if (Arguments.Count > 3)
            {
                Log.WriteError("There are too many arguments for the rollback command.");
                return false;
            }

            return true;
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
                using (DbCommand cmd = DataAccess.CreateCommand())
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