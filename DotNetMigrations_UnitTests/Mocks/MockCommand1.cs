using System.Collections.Generic;
using System.ComponentModel.Composition;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.MockCommands
{
    [Export("Commands", typeof(ICommand))]
    internal class MockCommand1 : CommandBase
    {
        private string _commandName = "TestCommand";
        private string _helpText = "This is help text for MockCommand1.";
        private IList<ICommand> _subcommands = new List<ICommand>();

        public override string CommandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        public override string HelpText 
        { 
            get { return _helpText;}
            set { _helpText = value; }
        }

        [ImportMany("MockSubcommand", typeof(ICommand))]
        public override IList<ICommand> SubCommands 
        { 
            get { return _subcommands;} 
            set { _subcommands = value;} 
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
