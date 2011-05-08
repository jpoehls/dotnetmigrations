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
    public class GenerateScriptCommandUnitTests
    {
        [Test]
        public void Run_should_call_IMigrationDirectory_CreateBlankScript_with_correct_args()
        {
            //  arrange
            var mockMigrationDir = new Mock<IMigrationDirectory>();
            mockMigrationDir.Setup(dir => dir.CreateBlankScript("my_name")).Returns("C:\\1234_my_name.sql");

            var cmd = new GenerateScriptCommand(mockMigrationDir.Object);
            cmd.Log = new MockLog1();
            cmd.MigrationName = "my_name";

            //  act
            cmd.Run();

            //  assert
            mockMigrationDir.Verify(dir => dir.CreateBlankScript("my_name"));
        }

        [Test]
        public void Run_should_log_file_name_of_new_migration_script()
        {
            //  arrange
            var mockMigrationDir = new Mock<IMigrationDirectory>();
            mockMigrationDir.Setup(dir => dir.CreateBlankScript("my_name")).Returns("C:\\1234_my_name.sql");

            var cmd = new GenerateScriptCommand(mockMigrationDir.Object);
            var mockLog = new MockLog1();
            cmd.Log = mockLog;
            cmd.MigrationName = "my_name";

            //  act
            cmd.Run();

            //  assert
            mockLog.Output.Contains(" 1234_my_name.sql ");
        }

        [Test]
        public void Validation_should_fail_if_MigrationName_is_null_or_empty()
        {
            //  arrange
            var validator = new DotConsole.DataAnnotationValidator();
            var cmd = new GenerateScriptCommand();

            //  act
            bool valid = validator.ValidateParameters(cmd);

            //  assert
            Assert.IsFalse(valid);
            Assert.AreEqual(1, validator.ErrorMessages.Count());
        }

        [Test]
        public void Validation_should_succeed_if_MigrationName_has_value()
        {
            //  arrange
            var validator = new DotConsole.DataAnnotationValidator();
            var cmd = new GenerateScriptCommand();
            cmd.MigrationName = "something";

            //  act
            bool valid = validator.ValidateParameters(cmd);

            //  assert
            Assert.IsTrue(valid);
        }
    }
}