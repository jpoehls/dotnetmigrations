using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class VersionCommandIntegrationTests : DatabaseIntegrationTests
    {
        private string _migrationPath;
        private string _scriptName;
        private string _dbVersion;

        private DatabaseCommandArguments _commandArgs;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            _commandArgs = new DatabaseCommandArguments();
            _commandArgs.Connection = TestConnectionString;

            //SetupTestScript();
            //SetupDatabase();
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
        public void Run_should_log_latest_script_version()
        {
            //  arrange
            var scriptFiles = new MigrationScriptFile[]
                                  {
                                      new MigrationScriptFile("C:\\3_third.sql"),
                                      new MigrationScriptFile("C:\\1_first.sql"),
                                      new MigrationScriptFile("C:\\2_second.sql")
                                  };

            var mockMigrationDir = new Mock<IMigrationDirectory>();
            mockMigrationDir.Setup(dir => dir.GetScripts()).Returns(scriptFiles);

            var mockLog = new MockLog1();
            var cmd = new VersionCommand(mockMigrationDir.Object);
            cmd.Log = mockLog;

            //  act
            cmd.Run(_commandArgs);

            //  assert
            Assert.IsTrue(mockLog.Output.Contains("Current script version:".PadRight(30) + "3"));
        }

        [Test]
        public void Run_should_log_current_database_schema_version()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_return_0_if_schema_migrations_table_doesnt_exist()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_create_schema_migrations_table_if_it_doesnt_exist()
        {
            throw new NotImplementedException();
        }
    }
}