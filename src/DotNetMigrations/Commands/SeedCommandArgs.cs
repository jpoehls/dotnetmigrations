using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public class SeedCommandArgs : DatabaseCommandArguments
    {
        [Argument("set", "s", "Set of seed data to plant.", Position = 2)]
        public string Set { get; set; }
    }
}