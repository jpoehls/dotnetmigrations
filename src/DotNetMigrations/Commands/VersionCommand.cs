using System;
using System.ComponentModel;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    [DotConsole.Command("version")]
    [Description("Displays the latest version of the database and the migration scripts.")]
    public class VersionCommand : DatabaseCommandBase
    {
        private readonly IMigrationDirectory _migrationDirectory;

        public VersionCommand()
            : this(new MigrationDirectory())
        {
        }

        public VersionCommand(IMigrationDirectory migrationDirectory)
        {
            _migrationDirectory = migrationDirectory;
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        public override void Execute()
        {
            // Obtain Latest Script Version
            long scriptVersion = GetLatestScriptVersion();

            // Obtain Latest Database Version
            long databaseVersion = GetDatabaseVersion();

            Log.WriteLine("Database is at version:".PadRight(30) + databaseVersion);
            Log.WriteLine("Scripts are at version:".PadRight(30) + scriptVersion);

            if (databaseVersion == scriptVersion)
            {
                Log.WriteLine(string.Empty);
                Log.WriteLine("Your database is up-to-date!");
            }
        }

        /// <summary>
        /// Retrieves the latest migration script version from the migration directory.
        /// </summary>
        /// <returns>The latest script version</returns>
        private long GetLatestScriptVersion()
        {
            IOrderedEnumerable<IMigrationScriptFile> files = _migrationDirectory.GetScripts()
                .OrderByDescending(x => x.Version);

            IMigrationScriptFile latestFile = files.FirstOrDefault();

            if (latestFile != null)
            {
                return latestFile.Version;
            }

            return 0;
        }
    }
}