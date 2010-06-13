using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class RollbackCommandIntegrationTests : DatabaseIntegrationTests
    {
        private DatabaseCommandArguments _commandArgs;
        private RollbackCommand _rollbackCommand;
        private Mock<DatabaseCommandBase<MigrateCommandArgs>> _mockMigrateCommand;
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
            _rollbackCommand = new RollbackCommand(_mockMigrateCommand.Object);
            _rollbackCommand.Log = _mockLog;
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        [Test]
        public void Run_should_call_MigrateCommand_Run()
        {
            //  arrange
            bool runCalled = false;
            _mockMigrateCommand.Protected().Setup("Run", ItExpr.IsAny<MigrateCommandArgs>()).Callback(() => runCalled = true);

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.IsTrue(runCalled);
        }

        [Test]
        public void Run_should_log_message_if_there_is_no_previous_version_to_rollback_to()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_not_call_MigrateCommand_Run_if_there_is_no_previous_version_to_rollback_to()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_set_MigrateCommandArgs_TargetVersion_to_previous_schema_version_in_schema_migrations_table()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_set_MigrateCommand_Log_to_its_own_Log()
        {
            //  arrange
            _mockMigrateCommand.SetupProperty(x => x.Log);

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            Assert.AreSame(_rollbackCommand.Log, _mockMigrateCommand.Object.Log);
        }

        [Test]
        public void Run_should_set_MigrateCommandArgs_Connection_to_its_own_Connection()
        {
            //  arrange
            _mockMigrateCommand.SetupProperty(x => x.Log);
            _mockMigrateCommand.Protected().Setup("Run", ItExpr.IsAny<MigrateCommandArgs>())
                .Callback((MigrateCommandArgs args) =>
                Assert.AreEqual(_commandArgs.Connection, args.Connection));

            //  act
            _rollbackCommand.Run(_commandArgs);

            //  assert
            _mockMigrateCommand.Verify();
        }
    }
}