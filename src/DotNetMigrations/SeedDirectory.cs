using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations
{
    public class SeedDirectory : ISeedDirectory
    {
        private const string DefaultSeedScriptPath = @".\seeds\";
        private const string ScriptFileNamePattern = "*.sql";
        private readonly IConfigurationManager _configurationManager;

        public SeedDirectory()
            : this(new ConfigurationManagerWrapper())
        {
        }

        public SeedDirectory(IConfigurationManager configurationManager)
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
        /// Returns the seed script path from the
        /// config file (if available) or the default path.
        /// </summary>
        public string GetPath(ILogger log)
        {
            string path = _configurationManager.AppSettings[AppSettingKeys.SeedFolder];

            if (string.IsNullOrEmpty(path))
            {
                if (log != null)
                {
                    log.WriteWarning(
                        "The " + AppSettingKeys.SeedFolder + " setting was not present in the configuration file so the default " +
                        DefaultSeedScriptPath + " folder will be used instead.");
                }
                path = DefaultSeedScriptPath;
            }

            VerifyAndCreatePath(path);

            return path;
        }

        /// <summary>
        /// Returns a list of all the seed script file paths sorted by name.
        /// </summary>
        public IEnumerable<string> GetScripts(string setName)
        {
            IComparer<string> comparer = new NaturalSortComparer<string>();

            string seedPath = GetPath(null);
            IEnumerable<string> seedScripts = Directory.GetFiles(seedPath, ScriptFileNamePattern).OrderBy(x => x, comparer);

            if (!string.IsNullOrEmpty(setName))
            {
                string setPath = Path.Combine(seedPath, setName);
                if (Directory.Exists(setPath))
                {
                    seedScripts = seedScripts.Union(Directory.GetFiles(setPath, ScriptFileNamePattern).OrderBy(x => x, comparer));
                }
            }

            return seedScripts;
        }
    }
}