using System.Collections.Generic;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.MockCommands;
using NUnit.Framework;
using Rhino.Mocks;

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

            CommandRepository repository = new CommandRepository();
            MockCommand1 cmd = repository.GetCommand("TestCommand") as MockCommand1;

            if (cmd == null)
            {
                Assert.Fail("TestCommand wasn't found.");
            }

            // Empty the subcommands
            cmd.SubCommands.Clear();

            MockLog1 log = new MockLog1();
            HelpCommand help = new HelpCommand(log);
            help.ShowHelp(cmd);

            Assert.AreEqual(expected, log.Output);
        }

        [Test]
        public void Should_Be_Able_To_Output_Help_Text_For_A_Command_With_Subcommands()
        {
            string expected = "TestCommand\r\n\r\n"
                              + "This is help text for MockCommand1.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                              + "\r\n\r\n\r\n\r\n"
                              + "Subcommands for TestCommand:\r\n"
                              + string.Empty.PadLeft(60, '=')
                              + "\r\n\r\n"
                              + "TestSubcommand\r\n\r\n"
                              + "This is the help text for MockSubcommand.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                              + "\r\n\r\n\r\n";

            CommandRepository repository = new CommandRepository();
            MockCommand1 cmd = repository.GetCommand("TestCommand") as MockCommand1;

            if (cmd == null)
            {
                Assert.Fail("TestCommand wasn't found.");
            }

            MockLog1 log = new MockLog1();
            HelpCommand help = new HelpCommand(log);
            help.ShowHelp(cmd);

            Assert.AreEqual(expected, log.Output);
        }

        [Test]
        public void Should_Be_Able_To_Output_Help_Text_For_All_Commands()
        {
            string expected = "Generate\r\n\r\n"
                                + "Generates a new migration script in the migration direc".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                                + "\r\n"
                                + "tory.".Trim().PadRight(55, ' ').PadLeft(60, ' ')
                                + "\r\n\r\n\r\n"
                                + "TestCommand\r\n\r\n" 
                                + "This is help text for MockCommand1.".Trim().PadRight(55, ' ').PadLeft(60, ' ') 
                                + "\r\n\r\n\r\n";

            CommandRepository repository = new CommandRepository();
            MockLog1 log = new MockLog1();
            HelpCommand help = new HelpCommand(log);
            help.ShowHelp(repository.Commands);

            Assert.AreEqual(expected, log.Output);
        }
    }
}
