using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using DotNetMigrations.Migrations;
using System.Collections.Generic;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class RollbackCommandIntegrationTests : DatabaseIntegrationTests
    {
        private DatabaseCommandArguments _commandArgs;
        private RollbackCommand _rollbackCommand;
        private Mock<DatabaseCommandBase<MigrateCommandArgs>> _mockMigrateCommand;
		private Mock<IMigrationDirectory> _mockMigrationDir;
		private List<IMigrationScriptFile> _mockMigrationScripts;
        private MockLog1 _mockLog;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _commandArgs = new DatabaseCommandArguments();
            _commandArgs.Connection = TestConnectionString;
        }

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mockLog = new MockLog1();

            _mockMigrateCommand = new Mock<DatabaseCommandBase<MigrateCommandArgs>>();
            _mockMigrateCommand.SetupProperty(x => x.Log);

			_mockMigrationScripts = new List<IMigrationScriptFile>();

			_mockMigrationDir = new Mock<IMigrationDirectory>();
			_mockMigrationDir.Setup(x => x.GetScripts()).Returns(() => _mockMigrationScripts);

            _rollbackCommand = new RollbackCommand(_mockMigrateCommand.Object, _mockMigrationDir.Object);
            _rollbackCommand.Log = _mockLog;

            CreateDatabase();
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        [Test]
        public void Run_should_call_MigrateCommand_Execute_if_there_is_a_previous_version_to_rollback_to()
        {
            //  arrange
            InitializeDatabase();

            //  insert some version numbers
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1)");
            }

            bool runCalled = false;
            _mockMigrateCommand.Protected().Setup("Execute", ItExpr.IsAny<MigrateCommandArgs>()).Callback(() => runCalled = true);

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.IsTrue(runCalled);
        }

        [Test]
        public void Run_should_log_message_if_there_is_no_previous_version_to_rollback_to()
        {
            //  arrange

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("No rollback is necessary. Database schema is already at version 0."));
        }

        [Test]
        public void Run_should_not_call_MigrateCommand_Execute_if_there_is_no_previous_version_to_rollback_to()
        {
            //  arrange
            bool runCalled = false;
            _mockMigrateCommand.Protected().Setup("Execute", ItExpr.IsAny<MigrateCommandArgs>()).Callback(() => runCalled = true);

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.IsFalse(runCalled);
        }

        [Test]
        public void Run_should_set_MigrateCommandArgs_TargetVersion_to_previous_schema_version_in_schema_migrations_table()
        {
            //  arrange
            InitializeDatabase();

            //  insert some version numbers
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1)");
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (2)");
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (3)");
            }

            _mockMigrateCommand.Protected().Setup("Execute", ItExpr.IsAny<MigrateCommandArgs>())
                .Callback((MigrateCommandArgs args) =>
                Assert.AreEqual(2, args.TargetVersion));

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            _mockMigrateCommand.Verify();
        }

        [Test]
        public void Run_should_set_MigrateCommand_Log_to_its_own_Log()
        {
            //  arrange
            InitializeDatabase();

            //  insert some version numbers
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1)");
            }

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.AreSame(_rollbackCommand.Log, _mockMigrateCommand.Object.Log);
        }

        [Test]
        public void Run_should_set_MigrateCommandArgs_Connection_to_its_own_Connection()
        {
            //  arrange
            _mockMigrateCommand.Protected().Setup("Execute", ItExpr.IsAny<MigrateCommandArgs>())
                .Callback((MigrateCommandArgs args) =>
                Assert.AreEqual(_commandArgs.Connection, args.Connection));

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            _mockMigrateCommand.Verify();
        }
    }
}