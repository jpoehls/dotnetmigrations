using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Provider;

namespace DotNetMigrations.UnitTests.Stubs
{
    public class InMemoryConfigurationManager : IConfigurationManager
    {
        public InMemoryConfigurationManager()
        {
            AppSettings = new NameValueCollection();
            ConnectionStrings = new ConnectionStringSettingsCollection();
        }

        #region IConfigurationManager Members

        public NameValueCollection AppSettings { get; private set; }
        public ConnectionStringSettingsCollection ConnectionStrings { get; private set; }

		public T GetSection<T>(string sectionname) where T : ConfigurationSection, new()
		{
			return new T();
		}

        #endregion
	}
}