using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            string scriptVersion = GetLatestScriptVersion().Trim();

            // Obtain Latest Database Version
            GetLatestDatabaseVersion().Trim();

            Log.WriteLine("Current Script Version:".PadRight(30) + scriptVersion);
        }

        /// <summary>
        /// Retrieves the latest migration script version from the migration directory.
        /// </summary>
        /// <returns>The latest script version</returns>
        private static string GetLatestScriptVersion()
        {
            const string scriptNamePattern = "*.sql";
            string migrationDirectory = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(migrationDirectory))
            {
                return "Migration directory not found.";
            }

            var files = new List<string>(Directory.GetFiles(migrationDirectory, scriptNamePattern));
            files.Sort();
            files.Reverse();

            string[] pathParts = files[0].Split(Path.PathSeparator);
            string fileName = pathParts[pathParts.Length - 1].Split('_')[0];

            return fileName;
        }

        /// <summary>
        /// Retrieves the current Database version.
        /// </summary>
        /// <returns>The current version of the database.</returns>
        private string GetLatestDatabaseVersion()
        {
            return GetDatabaseVersion().ToString();
        }
    }
}