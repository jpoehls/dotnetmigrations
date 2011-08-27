using System;

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
        }

        /// <summary>
        /// Creates the migration table into the database.
        /// </summary>
        private void CreateMigrationTable()
        {
            const string createTableCommand = "CREATE TABLE [schema_migrations]([id] INT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_schema_migrations] PRIMARY KEY, [version] [nvarchar](14) NOT NULL)";
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