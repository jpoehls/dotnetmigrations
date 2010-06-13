using System;
using System.Configuration;
using System.Linq;

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
    }
}