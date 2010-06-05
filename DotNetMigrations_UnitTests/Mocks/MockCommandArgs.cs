using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    public class MockCommandArgs : CommandArguments
    {
        [Argument("connection", "c", "Name of the connection to use",
            ValueName = "connection_value",
            Position = 1)]
        [Required(ErrorMessage = "Connection is required")]
        public string Connection { get; set; }

        [Argument("version", "v", "Version to migrate up or down to",
            Position = 2)]
        [Range(typeof(Int64), "1", "5",
            ErrorMessage = "Target version must be between 1 and 5")]
        public long TargetVersion { get; set; }
    }
}