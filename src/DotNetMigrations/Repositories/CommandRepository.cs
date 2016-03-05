using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetMigrations.Core;

namespace DotNetMigrations.Repositories
{
    public class CommandRepository
    {
        private readonly AggregateCatalog catalog;
        private readonly CompositionContainer container;
		private readonly IConfigurationManager configManager;

        /// <summary>
        /// A collection of the commands in the system.
        /// </summary>
        [ImportMany("Commands", typeof(ICommand))]
        public IList<ICommand> Commands { get; set; }

        /// <summary>
        /// Instantiates a new instance of the CommandRepository class.
        /// </summary>
		public CommandRepository(IConfigurationManager configManager)
        {
			this.configManager = configManager;

			var pluginDirectory = this.configManager.AppSettings[AppSettingKeys.PluginFolder];

            Commands = new List<ICommand>();

            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetCallingAssembly()));

            if (Directory.Exists(pluginDirectory))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(pluginDirectory));
            }

            container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        /// <summary>
        /// Retrieves the command based on the command name.
        /// </summary>
        /// <param name="commandName">The name of the command to retrieve.</param>
        /// <returns>An instance of the command or null if not found.</returns>
        public ICommand GetCommand(string commandName)
        {
            ICommand cmd = (from c in Commands
                    where c.CommandName.ToLowerInvariant() == commandName.ToLowerInvariant()
                    select c).FirstOrDefault();

            return cmd;
        }
    }
}
