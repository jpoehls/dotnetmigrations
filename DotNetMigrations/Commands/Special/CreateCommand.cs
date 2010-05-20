using System.Collections.Generic;
using System.Configuration;
using System.IO;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Logs;

namespace DotNetMigrations.Commands.Special
{
    internal class CreateCommand 
    {
        private ILogger log;
        private DataAccess dataAccess;

        /// <summary>
        /// Instantiates a new instance of the CreateCommand class.
        /// </summary>
        /// <remarks>Sets the default log to an instance of the ConsoleLog class.</remarks>
        internal CreateCommand()
            : this(new ConsoleLog())
        {
        }

        /// <summary>
        /// Instantiates a new instance of the CreateCommand class.
        /// </summary>
        /// <param name="logger">An instance of a class that implements the ILogger interface.</param>
        internal CreateCommand(ILogger logger)
        {
            log = logger;
            dataAccess = new DataAccess();
        }

        /// <summary>
        /// Creates the schema_migration table.
        /// </summary>
        /// <param name="migrationName">The name of the migration</param>
        /// <param name="connectionString">The connection string to use.</param>
        internal void Create(string migrationName, string connectionString)
        {
            // Do nothing if table already exists.
            if (MigrationTableExists(connectionString))
            {
                return;
            }

            // Create the table
            CreateMigrationTable(connectionString);

            // Migrate if the legacy table exists.
            if (LegacyTableExists(connectionString))
            {
                MigrateToNewTable(migrationName, connectionString);
            }
        }
        
        /// <summary>
        /// Checks to see if the migration table currently exists or not.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>True if the migration table current exists; else false.</returns>
        private bool MigrationTableExists(string connectionString)
        {
            string commandText = "SELECT [version] FROM [schema_migrations]";

            object tmp = dataAccess.ExecuteScalar(connectionString, commandText, true);

            return (tmp != null);
        }

        /// <summary>
        /// Checks to see if the old migration table currently exists or not.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>True if the old migration table current exists; else false.</returns>
        private bool LegacyTableExists(string connectionString)
        {
            string commandText = "SELECT [version] FROM [schema_info]";

            object tmp = dataAccess.ExecuteScalar(connectionString, commandText, true);

            return (tmp != null);
        }

        /// <summary>
        /// Creates the migration table into the database.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        private void CreateMigrationTable(string connectionString)
        {
            string createTableCommand = "CREATE TABLE [schema_migrations]([version] [char](14) NOT NULL PRIMARY KEY)";
            string firstRecordCommand = "INSERT INTO [schema_migrations] ([version]) VALUES (0)";

            dataAccess.ExecuteNonQuery(connectionString, createTableCommand);
            dataAccess.ExecuteNonQuery(connectionString, firstRecordCommand);
        }

        /// <summary>
        /// Migrates record information from the old migration table to the new one.
        /// </summary>
        /// <param name="migrationName">The name of the migration</param>
        /// <param name="connectionString">The connectionstring to use.</param>
        /// <remarks>This process assumes that no migration scripts were skipped.</remarks>
        private void MigrateToNewTable(string migrationName, string connectionString)
        {
            // Get Latest Version
            long currentVersion = GetCurrentVersion(connectionString);

            // Add Versions - Assume none were skipped.
            AddOldVersionsToNewTable(migrationName, connectionString, currentVersion);

            // Drop Old
            dataAccess.ExecuteNonQuery(connectionString, "DROP TABLE [schema_info]");
        }

        /// <summary>
        /// Retrieves the current version from the old migration table.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The version of the database.</returns>
        private long GetCurrentVersion(string connectionString)
        {
            string command = "SELECT MAX([version]) FROM [schema_info]";
            return dataAccess.ExecuteScalar<long>(connectionString, command);
        }

        /// <summary>
        /// Adds the version located in the migration directory that are lower or equal to the version of the old
        /// migration table to the new migration table.
        /// </summary>
        /// <param name="migrationName">The migration name.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="currentVersion">The version that current exists in the old migration table.</param>
        /// <remarks>This process assumes that no migration scripts were skipped.</remarks>
        private void AddOldVersionsToNewTable(string migrationName, string connectionString, long currentVersion)
        {
            string scriptNamePattern = "*" + migrationName + ".sql";
            string migrationDirectory = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(migrationDirectory))
            {
                return;
            }

            List<string> files = new List<string>(Directory.GetFiles(migrationDirectory, scriptNamePattern));
            files.Sort();

            string fileName;
            string sVers;
            long iVers;
            string command;

            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                sVers = fileName.Split('_')[0];

                if (long.TryParse(sVers, out iVers))
                {
                    if (iVers <= currentVersion)
                    {
                        command = "INSERT INTO [schema_migrations]([version]) VALUES (" + iVers + ")";
                        dataAccess.ExecuteNonQuery(connectionString, command);
                    }
                }
            }

        }
    }
}
