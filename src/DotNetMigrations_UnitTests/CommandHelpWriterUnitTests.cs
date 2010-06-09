using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class CommandHelpWriterUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _logger = new MockLog1();
            _helpWriter = new CommandHelpWriter(_logger);
        }

        [TearDown]
        public void Teardown()
        {
            _helpWriter = null;
            _logger.Dispose();
        }

        #endregion

        private MockLog1 _logger;
        private CommandHelpWriter _helpWriter;

        [Test]
        public void WriteArgumentList_should_include_all_options_with_their_Names_and_Description()
        {
            //  act
            _helpWriter.WriteArgumentList(typeof(MockCommandArgs));

            //  assert
            Assert.AreEqual("\r\nOptions:\r\n"
                            + "  -c, -connection    Name of the connection to use\r\n"
                            + "  -v, -version       Version to migrate up or down to\r\n",
                            _logger.Output);
        }

        [Test]
        public void WriteArgumentSyntax_should_include_all_arguments_with_brackets_around_optional_arguments()
        {
            //  act
            _helpWriter.WriteArgumentSyntax(typeof(MockCommandArgs));

            //  assert
            Assert.AreEqual("-c connection_value [-v version]", _logger.Output);
        }

        [Test]
        public void WriteArgumentSyntax_should_use_Name_as_the_value_name_when_ValueName_is_null()
        {
            //  act
            _helpWriter.WriteArgumentSyntax(typeof(MockCommandArgs));

            //  assert
            Assert.AreEqual("-c connection_value [-v version]", _logger.Output);
        }

        [Test]
        public void WriteArgumentSyntax_should_use_ValueName_when_provided()
        {
            //  act
            _helpWriter.WriteArgumentSyntax(typeof(MockCommandArgs));

            //  assert
            Assert.AreEqual("-c connection_value [-v version]", _logger.Output);
        }

        [Test]
        public void WriteCommandHelp_should_include_command_syntax_and_argument_list()
        {
            //  arrange
            var command = new MockCommand1();
            const string exeName = "test.exe";

            //  act
            string argumentSyntax = GetArgumentSyntax(command);
            string argumentList = GetArgumentList(command);
            _helpWriter.WriteCommandHelp(command, exeName);

            //  assert
            string expectedOutput = "\r\nUsage: " +
                                    exeName + " " + command.CommandName + " " + argumentSyntax + "\r\n" +
                                    argumentList;
            Assert.AreEqual(expectedOutput, _logger.Output);
        }

        [Test]
        public void WriteCommandList_should_include_all_commands_and_descriptions()
        {
            //  arrange
            var commandList = new List<ICommand> {
                new MockCommand1(),
                new MockCommand1()
            };
            int maxCommandNameLength = commandList.Max(x => x.CommandName.Length);

            //  act
            _helpWriter.WriteCommandList(commandList);

            //  assert
            string expectedOutput = "\r\nAvailable commands:\r\n" +
                                    "  " + commandList[0].CommandName.PadRight(maxCommandNameLength + 4) + commandList[0].Description + "\r\n" +
                                    "  " + commandList[1].CommandName.PadRight(maxCommandNameLength + 4) + commandList[1].Description + "\r\n";
            Assert.AreEqual(expectedOutput, _logger.Output);
        }

        private static string GetArgumentSyntax(ICommand command)
        {
            using (var logger = new MockLog1())
            {
                var writer = new CommandHelpWriter(logger);
                writer.WriteArgumentSyntax(command.GetArgumentsType());
                return logger.Output;
            }
        }

        private static string GetArgumentList(ICommand command)
        {
            using (var logger = new MockLog1())
            {
                var writer = new CommandHelpWriter(logger);
                writer.WriteArgumentList(command.GetArgumentsType());
                return logger.Output;
            }
        }
    }
}