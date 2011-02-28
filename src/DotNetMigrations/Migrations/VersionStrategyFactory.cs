using System;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Migrations
{
    public class VersionStrategyFactory
    {
        private readonly IConfigurationManager _configurationManager;

        public const string LocalTime = "local_time";
        public const string UtcTime = "utc_time";
        public const string SequentialNumber = "seq_num";

        public VersionStrategyFactory(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public IVersionStrategy GetStrategy()
        {
            string setting = _configurationManager.AppSettings[AppSettingKeys.VersionStrategy];

            if (string.Equals(LocalTime, setting, StringComparison.OrdinalIgnoreCase))
            {
                return new LocalTimestampVersion();
            }

            if (string.Equals(UtcTime, setting, StringComparison.OrdinalIgnoreCase))
            {
                return new UtcTimestampVersion();
            }

            if (string.Equals(SequentialNumber, setting, StringComparison.OrdinalIgnoreCase))
            {
                return new SequentialNumberVersion();
            }

            throw new ApplicationException(string.Format(
                "Invalid value proved for the versionStrategy appSetting. "
                + "Acceptable values are '{0}', '{1}', or '{2}'.",
                UtcTime, LocalTime, SequentialNumber));
        }
    }
}