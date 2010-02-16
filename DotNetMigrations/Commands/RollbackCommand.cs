using System.Data.Common;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class RollbackCommand : CommandBase
    {
        private string _commandName = "rollback";
        private string _helpText = "Rolls back the database by one version."
                                   + "\r\nExample: rollback <MigrateName> [ConnectionString]";
        private DataAccess _da;

        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }

        /// <summary>
        /// Instantiates a new instance of the RollbackCommand class.
        /// </summary>
        public RollbackCommand()
        {
            _da = new DataAccess();
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            var migrationName = GetMigrationName();
            var connectionString = GetConnectionString(migrationName);
            var currentVersion = GetDatabaseVersion(migrationName, connectionString);
            var previousVersion = GetPreviousDatabaseVersion(migrationName, connectionString, currentVersion);

            Arguments.Arguments.Add(previousVersion.ToString());

            var migrationCommand = new MigrateCommand();
            migrationCommand.Arguments = Arguments;
            migrationCommand.Log = Log;

            var results = migrationCommand.Run();

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
        /// db.exe migrate migrationName [version] [connectionstring]
        /// </remarks>
        protected override bool ValidateArguments()
        {
            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number of arguments for the rollback command is too few.");
                return false;
            }

            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count > 3)
            {
                Log.WriteError("There are too many arguments for the rollback command.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the Migration name from the arguments.
        /// </summary>
        /// <returns>The migration name as string.</returns>
        private string GetMigrationName()
        {
            return Arguments.GetArgument(1);
        }

        /// <summary>
        /// Retrieves the connection string from the command arguments or the config file.
        /// </summary>
        /// <param name="migrationName">The migration name used to identify the string in the config file.</param>
        /// <returns>The connection string.</returns>
        private string GetConnectionString(string migrationName)
        {
            string connArg = null;

            if (Arguments.Count == 3)
            {
                connArg = Arguments.GetArgument(2);
            }

            return _da.GetConnectionString(migrationName, connArg);
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <param name="migrationName">The migration name of the database to check.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The current version of the database.</returns>
        /// <remarks>If the [schema_migrations] database table doesn't exist, it will be created.</remarks>
        private long GetDatabaseVersion(string migrationName, string connectionString)
        {
            string command = "SELECT MAX([version]) FROM [schema_migrations]";

            long currentVersion = -1;
            string version = string.Empty;

            try
            {
                new CreateCommand(Log).Create(migrationName, connectionString);
                version = _da.ExecuteScalar<string>(connectionString, command).Trim();
                long.TryParse(version, out currentVersion);
            }
            catch (DbException ex)
            {
                Log.WriteError(ex.Message);
            }

            return currentVersion;
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <param name="migrationName">The migration name of the database to check.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The current version of the database.</returns>
        /// <remarks>If the [schema_migrations] database table doesn't exist, it will be created.</remarks>
        private long GetPreviousDatabaseVersion(string migrationName, string connectionString, long currentVersion)
        {
            string command = string.Format("SELECT MAX([version]) FROM [schema_migrations] WHERE [version] <> {0}", currentVersion.ToString());

            long previousVersion = -1;
            string version = string.Empty;

            try
            {
                new CreateCommand(Log).Create(migrationName, connectionString);
                version = _da.ExecuteScalar<string>(connectionString, command).Trim();
                long.TryParse(version, out previousVersion);
            }
            catch (DbException ex)
            {
                Log.WriteError(ex.Message);
            }

            return previousVersion;
        }
    }
}
