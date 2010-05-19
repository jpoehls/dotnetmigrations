using System.Collections.Generic;
using System.ComponentModel.Composition;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.MockCommands
{
    [Export("MockSubcommand", typeof(ICommand))]
    internal class MockSubcommand : CommandBase
    {
        public override string CommandName
        {
            get { return "TestSubcommand"; }
        }

        public override string HelpText
        {
            get { return "This is the help text for MockSubcommand."; }
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
