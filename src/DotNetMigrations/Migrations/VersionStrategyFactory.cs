using System;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Migrations
{
    public class VersionStrategyFactory
    {
        private readonly IConfigurationManager _configurationManager;

        public VersionStrategyFactory(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public IVersionStrategy GetStrategy()
        {
            string setting = _configurationManager.AppSettings["versionStrategy"];

            if (string.Equals("local_time", setting, StringComparison.OrdinalIgnoreCase))
            {
                return new LocalTimestampVersion();
            }

            if (string.Equals("utc_time", setting, StringComparison.OrdinalIgnoreCase))
            {
                return new UtcTimestampVersion();
            }

            throw new ApplicationException(
                "Invalid value proved for the versionStrategy appSetting. "
                + "Acceptable values are 'local_time' or 'utc_time'.");
        }
    }
}