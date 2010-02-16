using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;

namespace DotNetMigrations.Core.Provider
{
    public class ConfigurableTypeCatalog : TypeCatalog
    {
        public ConfigurableTypeCatalog()
            : base(GetTypes())
        {
        }

        public ConfigurableTypeCatalog(string sectionName)
            : base(GetTypes(sectionName))
        {            
        }

        private static IEnumerable<Type> GetTypes()
        {
            return GetTypes("mef.configurableTypes");
        }

        private static IEnumerable<Type> GetTypes(string sectionName)
        {
            var config = GetSection(sectionName);

            IList<Type> types = new List<Type>();

            foreach (ConfigurableTypeElement p in config.Parts)
            {
                types.Add(Type.GetType(p.Type));
            }

            return types;
        }

        private static ConfigurableTypeSection GetSection(string sectionName)
        {
            var config = ConfigurationManager.GetSection(sectionName) as ConfigurableTypeSection;

            if (config == null)
            {
                throw new ConfigurationErrorsException(string.Format("The configuration section {0} could not be found.", sectionName));
            }

            return config;
        }
    }
}
