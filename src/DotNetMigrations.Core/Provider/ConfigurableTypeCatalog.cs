using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;

namespace DotNetMigrations.Core.Provider
{
    public class ConfigurableTypeCatalog : TypeCatalog
    {
        public ConfigurableTypeCatalog(IConfigurationManager configManager)
			: base(GetTypes(configManager))
        {
        }

        public ConfigurableTypeCatalog(string sectionName, IConfigurationManager configManager)
			: base(GetTypes(configManager, sectionName))
        {
        }

		private static IEnumerable<Type> GetTypes(IConfigurationManager configManager)
        {
            return GetTypes(configManager, "mef.configurableTypes");
        }

        private static IEnumerable<Type> GetTypes(IConfigurationManager configManager, string sectionName)
        {
			var config = GetSection(configManager, sectionName);

            IList<Type> types = new List<Type>();

            foreach (ConfigurableTypeElement p in config.Parts)
            {
                types.Add(Type.GetType(p.Type));
            }

            return types;
        }

		private static ConfigurableTypeSection GetSection(IConfigurationManager configManager, string sectionName)
        {
			var config = configManager.GetSection<ConfigurableTypeSection>(sectionName);

            if (config == null)
            {
                throw new ConfigurationErrorsException(string.Format("The configuration section {0} could not be found.", sectionName));
            }

            return config;
        }
    }
}
