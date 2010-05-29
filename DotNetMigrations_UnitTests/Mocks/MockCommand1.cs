using System.Collections.Generic;
using System.ComponentModel.Composition;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    [Export("Commands", typeof(ICommand))]
    internal class MockCommand1 : CommandBase
    {
        private IList<ICommand> _subcommands = new List<ICommand>();

        public override string CommandName
        {
            get { return "TestCommand"; }
        }

        public override string HelpText
        {
            get { return "This is help text for MockCommand1."; }
        }

        [ImportMany("MockSubcommand", typeof(ICommand))]
        public override IList<ICommand> SubCommands
        {
            get { return _subcommands; }
            set { _subcommands = value; }
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