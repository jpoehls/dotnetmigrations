using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.UnitTests
{
    public abstract class DatabaseIntegrationTests
    {
        private readonly string _testDatabaseFile;

        protected DatabaseIntegrationTests()
        {
            _testDatabaseFile = Path.Combine(Path.GetTempPath(), "DotNetMigrationsTestDb.sdf");
            TestConnectionString = string.Format("Data Source={0};Persist Security Info=False;Provider=System.Data.SqlServerCe.3.5", _testDatabaseFile);
        }

        protected string TestConnectionString { get; private set; }

        /// <summary>
        /// Removes all objects in the database.
        /// </summary>
        protected void TeardownDatabase()
        {
            if (File.Exists(_testDatabaseFile))
            {
                File.Delete(_testDatabaseFile);
            }
        }

        protected void CreateDatabase()
        {
            if (!File.Exists(_testDatabaseFile))
            {
                using (var engine = new SqlCeEngine(SqlDatabaseHelper.GetConnectionStringWithoutProvider(TestConnectionString)))
                {
                    engine.CreateDatabase();
                }
            }
        }

        /// <summary>
        /// Calls DatabaseInitializer.Initialize() for the test database.
        /// </summary>
        protected void InitializeDatabase()
        {
            if (!File.Exists(_testDatabaseFile))
            {
                CreateDatabase();
            }

            using (var da = DataAccessFactory.Create(TestConnectionString))
            {
                da.OpenConnection();
                var dbInit = new DatabaseInitializer(da);
                dbInit.Initialize();
            }
        }
    }
}