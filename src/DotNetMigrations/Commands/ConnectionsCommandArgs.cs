using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class ConnectionsCommandArgs : CommandArguments
    {
        [ConnectionsCommandArgsActionValidator(ErrorMessage = "-action must be 'list', 'add', 'set', or 'remove'")]
        [Argument("action", "a", "Action to perform. [ list | add | set | remove ]",
            Position = 1)]
        public string Action { get; set; }

        [Argument("name", "n", "Name of connection to 'add' or 'set'.",
            Position = 2)]
        public string Name { get; set; }

        [Argument("connectionString", "cs", "Connection string to 'add' or 'set'.",
            Position = 3)]
        public string ConnectionString { get; set; }
    }
}