using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace DotNetMigrations.Core
{
	public class ConfigurationManagerWrapper : IConfigurationManager
	{
		protected static Lazy<Configuration> config = new Lazy<Configuration>(() =>
			{
				// If it exists - use the "local" configuration manager app settings
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				if(config.HasFile) return config;

				// If not, try the current path for a web.config file
				var wcfm = new WebConfigurationFileMap();
				wcfm.VirtualDirectories.Add("/", new VirtualDirectoryMapping(Environment.CurrentDirectory, true));
				config = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
				if(config.HasFile) return config;

				// If not, try the current path for a app.config file (prior to being built and moved to /bin)
				var ecfm = new ExeConfigurationFileMap();
				ecfm.ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, "app.config");
				config = ConfigurationManager.OpenMappedExeConfiguration(ecfm, ConfigurationUserLevel.None);
				if(config.HasFile) return config;

				throw new InvalidOperationException("No *.config file could be found that applies");
			});

		public NameValueCollection AppSettings
		{
			get
			{
				NameValueCollection appsettings = new NameValueCollection();
				var settings = config.Value.AppSettings.Settings;

				foreach(var key in settings.AllKeys)
					appsettings.Add(key, settings[key].Value);

				return appsettings;
			}
		}

		public ConnectionStringSettingsCollection ConnectionStrings
		{
			get
			{
				return config.Value.ConnectionStrings.ConnectionStrings;
			}
		}

		public static ConfigurationSection GetSection(string sectionname)
		{
			return config.Value.GetSection(sectionname);
		}
	}
}