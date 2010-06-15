using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            _commandArgs = new MigrateCommandArgs();
            _commandArgs.Connection = TestConnectionString;

            _mockLog = new MockLog1();

            _migrateCommand = new MigrateCommand(_mockMigrationDir.Object);
            _migrateCommand.Log = _mockLog;

            _mockMigrationScripts = new List<IMigrationScriptFile>();

            //  setup the mock migration scripts
            var mockScript1 = new Mock<IMigrationScriptFile>();
            mockScript1.SetupGet(x => x.Version).Returns(1);
            mockScript1.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               @"CREATE TABLE [TestTable] (Id INT NOT NULL)
                                                                GO
                                                                INSERT INTO [TestTable] (Id) VALUES (1)",
                                                               @"DROP TABLE [TestTable]"));
            _mockMigrationScripts.Add(mockScript1.Object);

            var mockScript2 = new Mock<IMigrationScriptFile>();
            mockScript2.SetupGet(x => x.Version).Returns(2);
            mockScript2.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               "INSERT INTO [TestTable] (Id) VALUES (2)",
                                                               "DELETE FROM [TestTable] WHERE Id = 2"));
            _mockMigrationScripts.Add(mockScript2.Object);
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
        private List<IMigrationScriptFile> _mockMigrationScripts;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _mockMigrationDir = new Mock<IMigrationDirectory>();
            _mockMigrationDir.Setup(x => x.GetScripts()).Returns(() => _mockMigrationScripts);
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

        [Test]
        public void Run_should_leave_schema_unchanged_if_migration_script_throws_exception_when_migrating_up()
        {
            //  arrange
            var mockErrorScript = new Mock<IMigrationScriptFile>();
            mockErrorScript.SetupGet(x => x.Version).Returns(3);
            mockErrorScript.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               "INSERT INTO [NonExistantTable] (Id) VALUES (1)",
                                                               "DELETE FROM [NonExistantTable] WHERE Id = 2"));
            _mockMigrationScripts.Add(mockErrorScript.Object);

            //  act
            try
            {
                _migrateCommand.Run(_commandArgs);
            }
            catch (SqlException)
            {
            }
            
            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(0, version, "schema version doesn't match the original schema version");

                var testTableCount =
                    sql.ExecuteScalar<int>("select count(*) from information_schema.tables where table_name='TestTable'");
                Assert.AreEqual(0, testTableCount, "not all migration script changes were rolled back");
            }
        }

        [Test]
        public void Run_should_leave_schema_unchanged_if_migration_script_throws_exception_when_migrating_down()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_wrap_any_SqlExceptions_thrown_into_MigrationExceptions()
        {
            //  todo: create a MigrationException class that wraps gives the migration filepath that had an error and sets inner exception to the sql exception that occurred
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_log_current_schema_version_before_performing_migrations()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_log_ending_schema_version_after_performing_migrations()
        {
            throw new NotImplementedException();
        }
    }
}