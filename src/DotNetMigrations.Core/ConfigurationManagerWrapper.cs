using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace DotNetMigrations.Core
{
    public class ConfigurationManagerWrapper : IConfigurationManager
    {
        public NameValueCollection AppSettings
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }

        public ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                return ConfigurationManager.ConnectionStrings;
            }
        }
    }
}