using System;
using System.Linq;

namespace DotNetMigrations.Migrations
{
    /// <summary>
    /// Holds the contents of a migration script file.
    /// </summary>
    public class MigrationScriptContents
    {
        public MigrationScriptContents(string setup, string teardown)
        {
            Setup = setup;
            Teardown = teardown;
        }

        public string Setup { get; private set; }
        public string Teardown { get; private set; }
    }
}