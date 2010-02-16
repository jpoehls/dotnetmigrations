using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class VersionCommand : CommandBase
    {
        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName { get; set;}

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string HelpText { get; set; }

        /// <summary>
        /// Instantiates a new instance of the VersionCommand class.
        /// </summary>
        public VersionCommand()
        {
            CommandName = "version";
            HelpText = "Displays the latest version of the database and the migration scripts."
                                   + "\r\nExample: version <MigrateName> [ConnectionString]";
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            string migrationName = Arguments.GetArgument(1);

            // Obtain Latest Script Version
            string scriptVersion = GetLatestScriptVersion(migrationName).Trim();

            // Obtain Latest Database Version
            string databaseVersion = GetLatestDatabaseVersion(migrationName).Trim();

            Log.WriteLine("Current Database Version:".PadRight(30) + databaseVersion);
            Log.WriteLine("Current Script Version:".PadRight(30) + scriptVersion);
        }

        /// <summary>
        /// Validates the command line arguments for this command.
        /// </summary>
        /// <returns>True if the arguments are considered to be valid.</returns>
        protected override bool ValidateArguments()
        {
            // The 1st argument is the command name and the 2nd is the migration name.
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number of arguments for the Version command is too few.");
                return false;
            }

            // The 3rd argument is the optional connection string.
            if (Arguments.Count > 3)
            {
                Log.WriteError("There are too many arguments designated for this command.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves teh latest migration script version from the migration directory.
        /// </summary>
        /// <param name="migrationName">The name of the migration.</param>
        /// <returns>The latest script version</returns>
        private string GetLatestScriptVersion(string migrationName)
        {
            string scriptNamePattern = "*" + migrationName + ".sql";
            string migrationDirectory = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(migrationDirectory))
            {
                return "Migration directory not found.";
            }

            List<string> files = new List<string>(Directory.GetFiles(migrationDirectory, scriptNamePattern));

            files.Sort();
            files.Reverse();

            var pathParts = files[0].Split('\\');
            var fileName = pathParts[pathParts.Length - 1].Split('_')[0];

            return fileName;
        }

        /// <summary>
        /// Retrieves the current Database version.
        /// </summary>
        /// <param name="migrationName">The name of the migration</param>
        /// <returns>The current version of the database.</returns>
        private string GetLatestDatabaseVersion(string migrationName)
        {
            DataAccess da = new DataAccess();

            string connArg = null;

            if(Arguments.Count == 3)
            {
                connArg = Arguments.GetArgument(2);
            }

            string connectionString = da.GetConnectionString(migrationName, connArg);

            string command = "SELECT MAX([version]) FROM [schema_migrations]";

            string version;

            try
            {
                new CreateCommand(Log).Create(migrationName, connectionString);
                version = da.ExecuteScalar<string>(connectionString, command);
            }
            catch (DbException ex)
            {
                version = ex.Message;
                Log.WriteError(ex.Message);
            }
            return version;
        }
        
    }
}
