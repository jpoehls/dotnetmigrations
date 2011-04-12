using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    public static class AppSettingKeys
    {
        /// <summary>
        /// The path to where the migration scripts are stored.
        /// </summary>
        public const string MigrateFolder = "migrateFolder";

        /// <summary>
        /// The path to where the seed scripts are stored.
        /// </summary>
        public const string SeedFolder = "seedFolder";

        public const string VersionStrategy = "versionStrategy";

        /// <summary>
        /// The path to where your DNM plugins are loaded from.
        /// </summary>
        public const string PluginFolder = "pluginFolder";

        /// <summary>
        /// Defines whether you want the stack traces logged or just the messages.
        /// </summary>
        public const string LogFullErrors = "logFullErrors";
    }
}