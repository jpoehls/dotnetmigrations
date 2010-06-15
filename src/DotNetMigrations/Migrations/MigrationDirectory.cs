using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetMigrations.Core;

namespace DotNetMigrations.Migrations
{
    public class MigrationDirectory : IMigrationDirectory
    {
        private const string DefaultMigrationScriptPath = @".\migrate\";
        private const string ScriptFileNamePattern = "*.sql";
        private readonly IConfigurationManager _configurationManager;

        public MigrationDirectory()
            : this(new ConfigurationManagerWrapper())
        {
        }

        public MigrationDirectory(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        /// <summary>
        /// Verify the path exists and creates it if it's missing.
        /// </summary>
        /// <param name="path">The path to verify.</param>
        private static void VerifyAndCreatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Returns the migration script path from the
        /// config file (if available) or the default path.
        /// </summary>
        public string GetPath(ILogger log)
        {
            const string appSettingName = "migrateFolder";
            string path = _configurationManager.AppSettings[appSettingName];

            if (string.IsNullOrEmpty(path))
            {
                if (log != null)
                {
                    log.WriteWarning(
                        "The " + appSettingName + " setting was not present in the configuration file so the default " +
                        DefaultMigrationScriptPath + " folder will be used instead.");
                }
                path = DefaultMigrationScriptPath;
            }

            VerifyAndCreatePath(path);

            return path;
        }

        /// <summary>
        /// Returns a list of all the migration script file paths
        /// sorted by version number (ascending).
        /// </summary>
        public IEnumerable<IMigrationScriptFile> GetScripts()
        {
            string[] files = Directory.GetFiles(GetPath(null), ScriptFileNamePattern);

            if (files != null)
            {
                return files.Select(x => (IMigrationScriptFile)new MigrationScriptFile(x)).OrderBy(x => x.Version);
            }

            return Enumerable.Empty<IMigrationScriptFile>();
        }

        /// <summary>
        /// Creates a blank migration script with the given name.
        /// </summary>
        /// <param name="migrationName">name of the migration script</param>
        /// <returns>The path to the new migration script.</returns>
        public string CreateBlankScript(string migrationName)
        {
            long version = GetNewVersionNumber();

            string path = GetPath(null);
            path = Path.Combine(path, version + "_" + SanitizeMigrationName(migrationName) + ".sql");

            var contents = new MigrationScriptContents(Environment.NewLine, Environment.NewLine);
            
            var file = new MigrationScriptFile(path);
            file.Write(contents);

            return path;
        }

        /// <summary>
        /// Returns a file name friendly version of the given
        /// migration name.
        /// </summary>
        private static string SanitizeMigrationName(string migrationName)
        {
            const char invalidCharReplacement = '-';

            //  replace the invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                migrationName = migrationName.Replace(c, invalidCharReplacement);
            }

            //  trim whitespace
            migrationName = migrationName.Trim();

            //  replace whitespace with an underscore
            const string whitespaceReplacement = "_";
            migrationName = Regex.Replace(migrationName, @"\s+", whitespaceReplacement, RegexOptions.Compiled);

            return migrationName;
        }

        /// <summary>
        /// Generates a new version number for assignment.
        /// </summary>
        private long GetNewVersionNumber()
        {
            var factory = new VersionStrategyFactory(_configurationManager);
            IVersionStrategy strategy = factory.GetStrategy();
            long version = strategy.GetNewVersionNumber();
            return version;
        }
    }
}