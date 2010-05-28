using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public static class MigrationScriptHelper
    {
        private const string DefaultMigrationScriptPath = @".\migrate\";
        private const string ScriptFileNamePattern = "*.sql";

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
        public static string GetScriptPath(ILogger log)
        {
            const string appSettingName = "migrateFolder";
            string path = ConfigurationManager.AppSettings[appSettingName];

            if (string.IsNullOrEmpty(path))
            {
                if (log != null)
                {
                    log.WriteWarning(
                        "The " + appSettingName + " setting was not present in the configuration file so the default " + DefaultMigrationScriptPath + " folder will be used instead.");
                }
                path = DefaultMigrationScriptPath;
            }

            VerifyAndCreatePath(path);

            return path;
        }

        /// <summary>
        /// Returns a list of all the migration script file paths.
        /// </summary>
        public static IEnumerable<MigrationScriptFile> GetScriptFiles()
        {
            var files = Directory.GetFiles(GetScriptPath(null), ScriptFileNamePattern);

            if (files != null)
            {
                return files.Select(x => new MigrationScriptFile(x));
            }

            return Enumerable.Empty<MigrationScriptFile>();
        }
    }
}