using System;
using System.Configuration;

namespace DotNetMigrations.Core.Provider
{
    public class ConfigurableTypeCollection : ConfigurationElementCollection 
    {
        protected override string ElementName
        {
            get
            {
                return "part";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfigurableTypeElement();
        }

		public void Add(ConfigurableTypeElement element)
		{
			BaseAdd(element);
		}

        protected override object GetElementKey(ConfigurationElement element)
        {
            var part = element as ConfigurableTypeElement;
            
            if (part != null)
            {
                return part.Type;
            }

            throw new InvalidOperationException();
        }
    }
}
