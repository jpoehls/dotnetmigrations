using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrateCommandIntegrationTests : DatabaseIntegrationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mockLog = new MockLog1();

            _migrateCommand = new MigrateCommand(_mockMigrationDir.Object);
            _migrateCommand.Log = _mockLog;
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        private MigrateCommandArgs _commandArgs;
        private MigrateCommand _migrateCommand;
        private MockLog1 _mockLog;
        private Mock<IMigrationDirectory> _mockMigrationDir;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _commandArgs = new MigrateCommandArgs();
            _commandArgs.Connection = TestConnectionString;

            //  setup the mock migration scripts
            var mockScript1 = new Mock<IMigrationScriptFile>();
            mockScript1.SetupGet(x => x.FilePath).Returns("C:\\1_add_table_and_initial_row.sql");
            mockScript1.SetupGet(x => x.Version).Returns(1);
            mockScript1.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               @"CREATE TABLE [TestTable] (Id INT NOT NULL)
                                                                GO
                                                                INSERT INTO [TestTable](Id) VALUES (1)",
                                                               @"DROP TABLE [TestTable]"));

            var mockScript2 = new Mock<IMigrationScriptFile>();
            mockScript2.SetupGet(x => x.FilePath).Returns("C:\\2_add_a_row.sql");
            mockScript2.SetupGet(x => x.Version).Returns(2);
            mockScript2.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               "INSERT INTO [TestTable] (Id) VALUES (2)",
                                                               "DELETE FROM [TestTable] WHERE Id = 2"));

            _mockMigrationDir = new Mock<IMigrationDirectory>();
            _mockMigrationDir.Setup(x => x.GetScripts()).Returns(() => new[]
                                                                           {
                                                                               mockScript1.Object,
                                                                               mockScript2.Object
                                                                           });
        }

        [Test]
        public void Run_should_migrate_down_to_TargetVersion_if_less_than_current_schema_version()
        {
            //  arrange
            //  migrate up first
            _migrateCommand.Run(_commandArgs);

            //  act
            _commandArgs.TargetVersion = 1;
            _migrateCommand.Run(_commandArgs);

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(1, version, "schema version doesn't match TargetVersion");

                var testTableId = sql.ExecuteScalar<int>("select max(Id) from [TestTable]");
                Assert.AreEqual(1, testTableId, "not all migration scripts were run as expected");
            }
        }

        [Test]
        public void Run_should_migrate_to_latest_script_version_if_no_TargetVersion_is_given()
        {
            //  arrange

            //  act
            _migrateCommand.Run(_commandArgs);

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(2, version, "schema version doesn't match latest script version");

                var testTableId = sql.ExecuteScalar<int>("select max(Id) from [TestTable]");
                Assert.AreEqual(2, testTableId, "not all migration scripts were run as expected");
            }
        }

        [Test]
        public void Run_should_migrate_up_to_TargetVersion_if_greater_than_current_schema_version()
        {
            //  arrange
            //  migrate to version 1 first
            _commandArgs.TargetVersion = 1;
            _migrateCommand.Run(_commandArgs);

            _commandArgs.TargetVersion = 2;

            //  act
            _migrateCommand.Run(_commandArgs);

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(2, version, "schema version doesn't match TargetVersion");

                var testTableId = sql.ExecuteScalar<int>("select max(Id) from [TestTable]");
                Assert.AreEqual(2, testTableId, "not all migration scripts were run as expected");
            }
        }

        [Test]
        public void Run_should_rollback_all_migrations_if_TargetVersion_is_0()
        {
            //  arrange
            //  migrate up first
            _migrateCommand.Run(_commandArgs);

            _commandArgs.TargetVersion = 0;

            //  act
            _migrateCommand.Run(_commandArgs);

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(0, version, "schema version doesn't match TargetVersion");

                var testTableCount =
                    sql.ExecuteScalar<int>("select count(*) from information_schema.tables where table_name='TestTable'");
                Assert.AreEqual(0, testTableCount, "not all migration scripts were run as expected");
            }
        }
    }
}