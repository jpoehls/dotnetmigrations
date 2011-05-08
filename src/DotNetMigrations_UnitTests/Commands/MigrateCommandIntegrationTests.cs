using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            _mockMigrationScripts = new List<IMigrationScriptFile>();

            _mockMigrationDir = new Mock<IMigrationDirectory>();
            _mockMigrationDir.Setup(x => x.GetScripts()).Returns(() => _mockMigrationScripts);

            _migrateCommand = new MigrateCommand(_mockMigrationDir.Object);
            _migrateCommand.Log = _mockLog;
            _migrateCommand.Connection = TestConnectionString;

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

            //  setup a migration script that throws an exception during Setup, but don't add it to the scripts collection
            _mockScriptWithBadSetup = new Mock<IMigrationScriptFile>();
            _mockScriptWithBadSetup.SetupGet(x => x.Version).Returns(3);
            _mockScriptWithBadSetup.SetupGet(x => x.FilePath).Returns("C:\\3_my_bad_script.sql");
            _mockScriptWithBadSetup.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               "INSERT INTO [NonExistantTable] (Id) VALUES (1)",
                                                               null));

            //  setup a migration script that throws an exception during Teardown, but don't add it to the scripts collection
            _mockScriptWithBadTeardown = new Mock<IMigrationScriptFile>();
            _mockScriptWithBadTeardown.SetupGet(x => x.Version).Returns(4);
            _mockScriptWithBadTeardown.SetupGet(x => x.FilePath).Returns("C:\\4_my_bad_script.sql");
            _mockScriptWithBadTeardown.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
                                                               "INSERT INTO [TestTable] (Id) VALUES (4)",
                                                               "DELETE FROM [NonExistantTable] WHERE Id = 4"));
            CreateDatabase();
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        private MigrateCommand _migrateCommand;
        private MockLog1 _mockLog;
        private Mock<IMigrationDirectory> _mockMigrationDir;
        private List<IMigrationScriptFile> _mockMigrationScripts;
        private Mock<IMigrationScriptFile> _mockScriptWithBadSetup;
        private Mock<IMigrationScriptFile> _mockScriptWithBadTeardown;

        [Test]
        public void Constructor_should_default_TargetVersion_to_negative_1()
        {
            //  assert
            Assert.AreEqual(-1, _migrateCommand.TargetVersion);
        }

        [Test]
        public void Run_should_migrate_down_to_TargetVersion_if_less_than_current_schema_version()
        {
            //  arrange
            //  migrate up first
            _migrateCommand.Run();

            //  act
            _migrateCommand.TargetVersion = 1;
            _migrateCommand.Run();

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
            _migrateCommand.Run();

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
            _migrateCommand.TargetVersion = 1;
            _migrateCommand.Run();

            _migrateCommand.TargetVersion = 2;

            //  act
            _migrateCommand.Run();

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
            _migrateCommand.Run();

            _migrateCommand.TargetVersion = 0;

            //  act
            _migrateCommand.Run();

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
            //  migrate up first
            _migrateCommand.Run();

            _migrateCommand.TargetVersion = 0;

            _mockMigrationScripts.Add(_mockScriptWithBadSetup.Object);

            //  act
            try
            {
                _migrateCommand.Run();
            }
            catch (MigrationException)
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
            //  arrange
            _mockMigrationScripts.Add(_mockScriptWithBadTeardown.Object);
            //  migrate up first
            _migrateCommand.Run();

            _migrateCommand.TargetVersion = 0;

            //  act
            try
            {
                _migrateCommand.Run();
            }
            catch (MigrationException)
            {
            }

            //  assert
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                var version =
                    sql.ExecuteScalar<long>(
                        "select max(version) from schema_migrations");

                Assert.AreEqual(4, version, "schema version doesn't match the original schema version");

                var testTableCount =
                    sql.ExecuteScalar<int>("select count(*) from information_schema.tables where table_name='TestTable'");
                Assert.AreEqual(1, testTableCount, "schema changed");
            }
        }

        [Test]
        public void Run_should_wrap_any_Exception_thrown_while_executing_a_migration_script_into_a_MigrationException()
        {
            //  arrange
            _mockMigrationScripts.Add(_mockScriptWithBadSetup.Object);

            //  act
            try
            {
                _migrateCommand.Run();
            }
            catch (MigrationException ex)
            {
                Assert.AreEqual(ex.FilePath, _mockScriptWithBadSetup.Object.FilePath);
                Assert.IsNotNull(ex.InnerException, "InnerException was null");
                return;
            }

            //  assert
            Assert.Fail("MigrationException was not thrown.");
        }

        [Test]
        public void Run_should_not_throw_exception_if_there_arent_any_migration_scripts()
        {
            //  arrange
            _mockMigrationScripts.Clear();

            //  act
            _migrateCommand.Run();
        }

        [Test]
        public void Run_should_log_message_if_there_arent_any_migration_scripts()
        {
            //  arrange
            _mockMigrationScripts.Clear();

            //  act
            _migrateCommand.Run();

            //  assert
            Assert.AreEqual("No migration scripts were found.\r\n", _mockLog.Output);
        }

        [Test]
        public void Run_should_log_message_if_schema_is_already_at_TargetVersion()
        {
            //  arrange
            _migrateCommand.TargetVersion = 0;

            //  act
            _migrateCommand.Run();

            //  assert
            Assert.IsTrue(Regex.IsMatch(_mockLog.Output, @"Database is at version:\s*0\r\n"));
        }

        [Test]
        public void Run_should_log_current_schema_version_before_performing_migrations()
        {
            //  arrange

            //  act
            _migrateCommand.Run();

            //  assert
            Assert.IsTrue(_mockLog.Output.StartsWith("Database is at version:".PadRight(30) + 0 + "\r\n"));
        }

        [Test]
        public void Run_should_log_TargetVersion_after_performing_migration_up()
        {
            //  arrange

            //  act
            _migrateCommand.Run();

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("Migrated up to version:".PadRight(30) + 2 + "\r\n"));
        }

        [Test]
        public void Run_should_log_TargetVersion_after_performing_migration_down()
        {
            //  arrange
            //  migrate up first
            _migrateCommand.Run();

            _migrateCommand.TargetVersion = 0;
            _mockLog.Clear();

            //  act
            _migrateCommand.Run();

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("Migrated down to version:".PadRight(30) + 0 + "\r\n"));
        }
    }
}