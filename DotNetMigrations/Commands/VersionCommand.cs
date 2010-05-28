using System;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class VersionCommand : DatabaseCommandBase
    {
        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return "version"; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string HelpText
        {
            get
            {
                return "Displays the latest version of the database and the migration scripts."
                       + "\r\nExample: version <MigrateName> [ConnectionString]";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            base.RunCommand();

            // Obtain Latest Script Version
            long scriptVersion = GetLatestScriptVersion();

            // Obtain Latest Database Version
            GetDatabaseVersion();

            Log.WriteLine("Current Script Version:".PadRight(30) + scriptVersion);
        }

        /// <summary>
        /// Retrieves the latest migration script version from the migration directory.
        /// </summary>
        /// <returns>The latest script version</returns>
        private static long GetLatestScriptVersion()
        {
            IOrderedEnumerable<MigrationScriptFile> files = MigrationScriptHelper.GetScriptFiles()
                .OrderByDescending(x => x);

            MigrationScriptFile latestFile = files.FirstOrDefault();

            if (latestFile != null)
            {
                return latestFile.Version;
            }

            return -1;
        }
    }
}