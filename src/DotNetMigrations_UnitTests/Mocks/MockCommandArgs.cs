using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    public class MockCommandArgs : CommandArguments
    {
        [Argument("version", "v", "Version to migrate up or down to",
            Position = 2)]
        [Range(typeof (Int64), "1", "5",
            ErrorMessage = "Target version must be between 1 and 5")]
        public long TargetVersion { get; set; }

        //  this property has a position of 1 and should be
        //  at the end of the class (after a property with a higher position)
        //  for accurate testing of certain methods
        [Argument("connection", "c", "Name of the connection to use",
            ValueName = "connection_value",
            Position = 1)]
        [Required(ErrorMessage = "Connection is required")]
        public string Connection { get; set; }

        //  property that doesn't have the [ArgumentAttribute]
        public string NotAnArgument { get; set; }
    }
}