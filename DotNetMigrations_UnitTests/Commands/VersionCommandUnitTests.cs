using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class VersionCommandUnitTests : DatabaseIntegrationTests
    {
        private string _migrationPath;
        private string _scriptName;
        private string _dbVersion;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            SetupTestScript();
            SetupDatabase();
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }

            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.ExecuteNonQuery("DROP TABLE [schema_migrations]");
            }
        }

        private void SetupTestScript()
        {
            _scriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_testDb.sql";
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(_migrationPath))
            {
                Directory.CreateDirectory(_migrationPath);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(_migrationPath, _scriptName)))
            {
                writer.WriteLine("This is a test file.");
            }
        }

        private void SetupDatabase()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.SwallowSqlExceptions = true;
                string results = helper.ExecuteScalar("SELECT MAX(version) FROM [schema_migrations]")
                    .ToString();

                if (string.IsNullOrEmpty(results))
                {
                    _dbVersion = "0";
                }
                else
                {
                    _dbVersion = results;
                }
            }
        }

        private bool EnsureTableWasCreated()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.SwallowSqlExceptions = true;
                string results = "SELECT MAX(version) FROM [schema_migrations]";
                return (!string.IsNullOrEmpty(results));
            }
        }

        [Test]
        public void Should_Report_DB_And_Script_Version()
        {
            var expected = new StringBuilder();
            expected.AppendLine(string.Format("Current Database Version:".PadRight(30) + "{0}", _dbVersion));
            expected.AppendLine(string.Format("Current Script Version:".PadRight(30) + "{0}", _scriptName.Split('_')[0]));

            var arguments = new[] {"Version", "testDb"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var versionCommand = new VersionCommand();
            versionCommand.Log = log;
            versionCommand.Arguments = args;

            CommandResults results = versionCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(log.Output.Length > 1);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.AreEqual(expected.ToString(), log.Output);
        }

        [Test]
        public void Should_Report_DB_And_Script_Version_Invalid_Arguments()
        {
            var arguments = new[] {"Version"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var versionCommand = new VersionCommand();
            versionCommand.Log = log;
            versionCommand.Arguments = args;

            CommandResults results = versionCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }
    }
}