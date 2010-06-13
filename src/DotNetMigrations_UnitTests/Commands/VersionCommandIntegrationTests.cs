using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class VersionCommandIntegrationTests : DatabaseIntegrationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mockLog = new MockLog1();
            _mockMigrationDir = new Mock<IMigrationDirectory>();
            _versionCommand = new VersionCommand(_mockMigrationDir.Object);
            _versionCommand.Log = _mockLog;
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        private Mock<IMigrationDirectory> _mockMigrationDir;
        private DatabaseCommandArguments _commandArgs;
        private VersionCommand _versionCommand;
        private MockLog1 _mockLog;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            _commandArgs = new DatabaseCommandArguments();
            _commandArgs.Connection = TestConnectionString;
        }

        [Test]
        public void Run_should_create_schema_migrations_table_if_it_doesnt_exist()
        {
            //  arrange
            _mockMigrationDir.Setup(dir => dir.GetScripts()).Returns(Enumerable.Empty<MigrationScriptFile>);

            //  act
            _versionCommand.Run(_commandArgs);

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var r =
                    (int)
                    sql.ExecuteScalar(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='schema_migrations'");
                Assert.IsTrue(r == 1);
            }
        }

        [Test]
        public void Run_should_log_current_database_schema_version_from_schema_migrations_table()
        {
            //  arrange
            _mockMigrationDir.Setup(dir => dir.GetScripts()).Returns(Enumerable.Empty<MigrationScriptFile>);

            //  initialze the database with the schema_migrations table
            using (DataAccess dataAccess = DataAccessFactory.Create(TestConnectionString))
            {
                dataAccess.OpenConnection();
                var dbInit = new DatabaseInitializer(dataAccess);
                dbInit.Initialize();
            }

            //  update schema_migrations table with a specific version number
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1234)");

                //  act
                _versionCommand.Run(_commandArgs);

                //  assert
                Assert.IsTrue(_mockLog.Output.Contains("Current database version:".PadRight(30) + "1234"));
            }
        }

        [Test]
        public void Run_should_log_database_schema_version_as_0_if_schema_migrations_table_doesnt_exist()
        {
            //  arrange
            _mockMigrationDir.Setup(dir => dir.GetScripts()).Returns(Enumerable.Empty<MigrationScriptFile>);

            //  act
            _versionCommand.Run(_commandArgs);

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("Current database version:".PadRight(30) + "0"));
        }

        [Test]
        public void Run_should_log_latest_script_version()
        {
            //  arrange
            var scriptFiles = new[]
                                  {
                                      new MigrationScriptFile("C:\\3_third.sql"),
                                      new MigrationScriptFile("C:\\1_first.sql"),
                                      new MigrationScriptFile("C:\\2_second.sql")
                                  };
            _mockMigrationDir.Setup(dir => dir.GetScripts()).Returns(scriptFiles);

            //  act
            _versionCommand.Run(_commandArgs);

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("Current script version:".PadRight(30) + "3"));
        }
    }
}