using System.Collections.Generic;
using System.ComponentModel.Composition;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.MockCommands
{
    [Export("MockSubcommand", typeof(ICommand))]
    internal class MockSubcommand : CommandBase
    {
        private string _commandName = "TestSubcommand";
        private string _helpText = "This is the help text for MockSubcommand.";

        public override string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        public override string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }

        protected override bool ValidateArguments()
        {
            return true;
        }

        protected override void RunCommand()
        {
            
        }
    }
}
