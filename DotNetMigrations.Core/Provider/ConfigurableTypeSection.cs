using System.Configuration;

namespace DotNetMigrations.Core.Provider
{
    public class ConfigurableTypeSection : ConfigurationSection
    {
        [ConfigurationProperty("parts", IsDefaultCollection = false)]
        public ConfigurableTypeCollection Parts
        {
            get { return (ConfigurableTypeCollection)base["parts"]; }
        }
    }
}
