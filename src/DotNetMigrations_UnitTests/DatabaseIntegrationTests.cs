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
    }
}