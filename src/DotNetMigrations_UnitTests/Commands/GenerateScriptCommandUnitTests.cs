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
            var cmdArgs = new GenerateScriptCommandArgs();
            cmdArgs.MigrationName = "my_name";

            //  act
            cmd.Run(cmdArgs);

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
            var cmdArgs = new GenerateScriptCommandArgs();
            cmdArgs.MigrationName = "my_name";

            //  act
            cmd.Run(cmdArgs);

            //  assert
            mockLog.Output.Contains(" 1234_my_name.sql ");
        }
    }
}