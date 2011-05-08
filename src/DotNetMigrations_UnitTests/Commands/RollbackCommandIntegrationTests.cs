using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class RollbackCommandIntegrationTests : DatabaseIntegrationTests
    {
        private RollbackCommand _rollbackCommand;
        private Mock<MigrateCommand> _mockMigrateCommand;
        private MockLog1 _mockLog;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mockLog = new MockLog1();

            _mockMigrateCommand = new Mock<MigrateCommand>();
            _mockMigrateCommand.SetupProperty(x => x.Log);

            _rollbackCommand = new RollbackCommand(_mockMigrateCommand.Object);
            _rollbackCommand.Log = _mockLog;
            _rollbackCommand.Connection = TestConnectionString;

            CreateDatabase();
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        [Test]
        public void Run_should_call_MigrateCommand_Run_if_there_is_a_previous_version_to_rollback_to()
        {
            //  arrange
            InitializeDatabase();

            //  insert some version numbers
            using (var sql = new SqlDatabaseHelper(TestConnectionString))
            {
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1)");
            }

            bool runCalled = false;
            _mockMigrateCommand.Setup(x => x.Run()).Callback(() => runCalled = true);

            //  act
            _rollbackCommand.Run();

            //  assert
            Assert.IsTrue(runCalled);
        }

        [Test]
        public void Run_should_log_message_if_there_is_no_previous_version_to_rollback_to()
        {
            //  arrange

            //  act
            _rollbackCommand.Run();

            //  assert
            Assert.IsTrue(_mockLog.Output.Contains("No rollback is necessary. Database schema is already at version 0."));
        }

        [Test]
        public void Run_should_not_call_MigrateCommand_Run_if_there_is_no_previous_version_to_rollback_to()
        {
            //  arrange
            bool runCalled = false;
            _mockMigrateCommand.Setup(x => x.Run()).Callback(() => runCalled = true);

            //  act
            _rollbackCommand.Run();

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
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (3)");
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (1)");
                sql.ExecuteNonQuery("insert into [schema_migrations] ([version]) values (2)");
            }

            _mockMigrateCommand.Setup(x=>x.Run())
                .Callback(() =>
                Assert.AreEqual(2, _mockMigrateCommand.Object.TargetVersion));

            //  act
            _rollbackCommand.Run();

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
            _rollbackCommand.Run();

            //  assert
            Assert.AreSame(_rollbackCommand.Log, _mockMigrateCommand.Object.Log);
        }

        [Test]
        public void Run_should_set_MigrateCommandArgs_Connection_to_its_own_Connection()
        {
            //  arrange
            _mockMigrateCommand.Setup(x=>x.Run())
                .Callback(() =>
                Assert.AreEqual(_rollbackCommand.Connection, _mockMigrateCommand.Object.Connection));

            //  act
            _rollbackCommand.Run();

            //  assert
            _mockMigrateCommand.Verify();
        }
    }
}