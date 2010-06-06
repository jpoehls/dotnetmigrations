using System;
using System.Linq;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class HelpCommandUnitTests
    {
        [Test]
        public void Should_Be_Able_To_Output_Help_Text_For_A_Command()
        {
            string expected = "TestCommand\r\n\r\n"
                              + "This is help text for MockCommand1.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                              + "\r\n\r\n\r\n";

            var repository = new CommandRepository();
            var cmd = repository.GetCommand("TestCommand") as MockCommand1;

            if (cmd == null)
            {
                Assert.Fail("TestCommand wasn't found.");
            }

            var log = new MockLog1();
            var help = new HelpCommand(log);
            help.ShowHelp(cmd);

            Assert.AreEqual(expected, log.Output);
        }

        [Test]
        public void Should_Be_Able_To_Output_Help_Text_For_All_Commands()
        {
            string expected = "Generate\r\n\r\n"
                              +
                              "Generates a new migration script in the migration direc".Trim().PadRight(55, ' ').PadLeft
                                  (60, ' ')
                              + "\r\n"
                              + "tory.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                              + "\r\n\r\n\r\n"
                              + "TestCommand\r\n\r\n"
                              + "This is help text for MockCommand1.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                              + "\r\n\r\n\r\n";

            var repository = new CommandRepository();
            var log = new MockLog1();
            var help = new HelpCommand(log);
            help.ShowHelp(repository.Commands);

            Assert.AreEqual(expected, log.Output);
        }
    }
}