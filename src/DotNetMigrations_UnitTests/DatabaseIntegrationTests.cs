using System;
using System.Configuration;
using System.Linq;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.UnitTests
{
    public abstract class DatabaseIntegrationTests
    {
        protected DatabaseIntegrationTests()
        {
            TestConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
        }

        protected string TestConnectionString { get; private set; }

        /// <summary>
        /// Removes all objects in the database.
        /// </summary>
        protected void TeardownDatabase()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.DropAllObjects();
            }
        }

        /// <summary>
        /// Calls DatabaseInitializer.Initialize() for the test database.
        /// </summary>
        protected void InitializeDatabase()
        {
            using (var da = DataAccessFactory.Create(TestConnectionString))
            {
                da.OpenConnection();
                var dbInit = new DatabaseInitializer(da);
                dbInit.Initialize();
            }   
        }
    }
}