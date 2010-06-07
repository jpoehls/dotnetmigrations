using System.Configuration;

namespace DotNetMigrations.Core.Provider
{
    public class ConfigurableTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true),
         StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
        public string Type
        {
            get { return this["type"] as string; }
            set { this["type"] = value;}
        }
    }
}
