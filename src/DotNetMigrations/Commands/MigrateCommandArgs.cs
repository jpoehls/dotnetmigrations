using System;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public class MigrateCommandArgs : DatabaseCommandArguments
    {
        public MigrateCommandArgs()
        {
            TargetVersion = -1;
        }

        [Argument("version", "v", "Target version to migrate up or down to.",
            Position = 2)]
        public long TargetVersion { get; set; }
    }
}