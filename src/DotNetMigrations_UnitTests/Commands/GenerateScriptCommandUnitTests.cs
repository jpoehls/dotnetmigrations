using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class GenerateScriptCommandUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Test_Setup()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        #endregion

        private string _migrationPath;

        [TestFixtureSetUp]
        public void Test_Fixture_Setup()
        {
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        [TestFixtureTearDown]
        public void Test_Fixture_Teardown()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }
    }
}