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
            var setting = _configurationManager.AppSettings["versionStrategy"];
            if (string.Equals("local_time", setting))
            {
                return new LocalTimestampVersion();
            }
            return new UtcTimestampVersion();
        }
    }
}