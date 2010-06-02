using System.Collections.Generic;
using System.ComponentModel.Composition;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    [Export("Commands", typeof(ICommand))]
    internal class MockCommand1 : CommandBase
    {
        public override string CommandName
        {
            get { return "TestCommand"; }
        }

        public override string HelpText
        {
            get { return "This is help text for MockCommand1."; }
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