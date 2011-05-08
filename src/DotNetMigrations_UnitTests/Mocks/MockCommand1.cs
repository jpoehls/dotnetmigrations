using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotConsole;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    [Command("TestCommand")]
    [Description("This is help text for MockCommand1.")]
    internal class MockCommand1 : CommandBase
    {
        [Parameter("version", Flag = 'v', Position = 1)]
        [Range(typeof(Int64), "1", "5",
            ErrorMessage = "Target version must be between 1 and 5")]
        [Description("Version to migrate up or down to")]
        public long TargetVersion { get; set; }

        //  this property has a position of 1 and should be
        //  at the end of the class (after a property with a higher position)
        //  for accurate testing of certain methods
        [Parameter("connection", Flag = 'c', MetaName = "connection_value", Position = 0)]
        [Required(ErrorMessage = "Connection is required")]
        [Description("Name of the connection to use")]
        public string Connection { get; set; }

        //  property that doesn't have the [ArgumentAttribute]
        public string NotAnArgument { get; set; }

        public bool RunShouldThrowException { get; set; }

        public override void Execute()
        {
            if (RunShouldThrowException)
            {
                throw new ApplicationException("error!");
            }
        }
    }
}