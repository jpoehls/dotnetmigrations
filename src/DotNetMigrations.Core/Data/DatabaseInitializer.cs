using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DotNetMigrations.Core.Data
{
    /// <summary>
    /// Initializes the database schema with objects
    /// required by DNM.
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly DataAccess _dataAccess;

        public DatabaseInitializer(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// Initializes the database schema by creating objects
        /// required by DNM.
        /// </summary>
        public void Initialize()
        {
            // Do nothing if table already exists.
            if (MigrationTableExists())
            {
                return;
            }

            // Create the table
            CreateMigrationTable();

            // Migrate if the legacy table exists.
            if (LegacyTableExists())
            {
                MigrateToNewTable();
            }
        }

        /// <summary>
        /// Migrates record information from the old migration table to the new one.
        /// </summary>
        /// <remarks>This process assumes that no migration scripts were skipped.</remarks>
        private void MigrateToNewTable()
        {
            // Get Latest Version
            long currentVersion = GetCurrentVersion();

            // Add Versions - Assume none were skipped.
            AddOldVersionsToNewTable(currentVersion);

            // Drop Old
            using (var cmd = _dataAccess.CreateCommand())
            {
                cmd.CommandText = "DROP TABLE [schema_info]";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Checks to see if the old migration table currently exists or not.
        /// </summary>
        /// <returns>True if the old migration table current exists; else false.</returns>
        private bool LegacyTableExists()
        {
            return TableExists("schema_info");
        }

        /// <summary>
        /// Retrieves the current version from the old migration table.
        /// </summary>
        /// <returns>The version of the database.</returns>
        private long GetCurrentVersion()
        {
            const string cmdText = "SELECT MAX([version]) FROM [schema_info]";

            using (var cmd = _dataAccess.CreateCommand())
            {
                cmd.CommandText = cmdText;
                return cmd.ExecuteScalar<long>();
            }
        }

        /// <summary>
        /// Adds the version located in the migration directory that are lower or equal to the version of the old
        /// migration table to the new migration table.
        /// </summary>
        /// <param name="currentVersion">The version that current exists in the old migration table.</param>
        /// <remarks>This process assumes that no migration scripts were skipped.</remarks>
        private void AddOldVersionsToNewTable(long currentVersion)
        {
            const string scriptNamePattern = "*.sql";
            string migrationDirectory = ConfigurationManager.AppSettings[AppSettingKeys.MigrateFolder];

            if (!Directory.Exists(migrationDirectory))
            {
                return;
            }

            var files = new List<string>(Directory.GetFiles(migrationDirectory, scriptNamePattern));
            files.Sort();

            string fileName;

            using (var tran = _dataAccess.BeginTransaction())
            {
                try
                {
                    foreach (string file in files)
                    {
                        fileName = Path.GetFileName(file);
                        string sVers = fileName.Split('_')[0];

                        long iVers;
                        if (long.TryParse(sVers, out iVers))
                        {
                            if (iVers <= currentVersion)
                            {
                                var cmdText = "INSERT INTO [schema_migrations]([version]) VALUES (" + iVers + ")";
                                using (var cmd = tran.CreateCommand())
                                {
                                    cmd.CommandText = cmdText;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates the migration table into the database.
        /// </summary>
        private void CreateMigrationTable()
        {
            const string createTableCommand = "CREATE TABLE [schema_migrations]([version] [nvarchar](14) NOT NULL PRIMARY KEY)";
            const string firstRecordCommand = "INSERT INTO [schema_migrations] ([version]) VALUES (0)";

            using (var tran = _dataAccess.BeginTransaction())
            {
                try
                {
                    using (var cmd = tran.CreateCommand())
                    {
                        cmd.CommandText = createTableCommand;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = tran.CreateCommand())
                    {
                        cmd.CommandText = firstRecordCommand;
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns true/false whether a table with the given
        /// name exists in the database.
        /// 
        /// Throws a SchemaException if more than one
        /// table is found with the given name.
        /// </summary>
        /// <exception cref="SchemaException" />
        private bool TableExists(string tableName)
        {
            const string cmdText = "select count(*) from [INFORMATION_SCHEMA].[TABLES] where [TABLE_NAME] = '{0}'";

            using (var cmd = _dataAccess.CreateCommand())
            {
                cmd.CommandText = string.Format(cmdText, tableName);
                var count = cmd.ExecuteScalar<int>();

                if (count == 1)
                {
                    return true;
                }

                if (count > 1)
                {
                    throw new SchemaException("More than one [" + tableName + "] table exists!");
                }

                return false;
            }
        }

        /// <summary>
        /// Checks to see if the migration table currently exists or not.
        /// </summary>
        /// <returns>True if the migration table current exists; else false.</returns>
        private bool MigrationTableExists()
        {
            return TableExists("schema_migrations");
        }
    }
}