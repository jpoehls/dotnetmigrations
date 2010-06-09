using System;
using System.ComponentModel.Composition;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    [Export("Commands", typeof (ICommand))]
    internal class MockCommand1 : CommandBase<MockCommandArgs>
    {
        public override string CommandName
        {
            get { return "TestCommand"; }
        }

        public override string Description
        {
            get { return "This is help text for MockCommand1."; }
        }

        public bool RunShouldThrowException { get; set; }

        protected override void Run(MockCommandArgs args)
        {
            if (RunShouldThrowException)
            {
                throw new ApplicationException("error!");
            }
        }
    }
}