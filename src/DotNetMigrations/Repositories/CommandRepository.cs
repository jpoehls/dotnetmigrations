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
    internal class CommandRepository
    {
        private readonly AggregateCatalog catalog;
        private readonly CompositionContainer container;

        /// <summary>
        /// A collection of the commands in the system.
        /// </summary>
        [ImportMany("Commands", typeof(ICommand))]
        internal IList<ICommand> Commands { get; set; }

        /// <summary>
        /// Instantiates a new instance of the CommandRepository class.
        /// </summary>
        internal CommandRepository()
        {
            var pluginDirectory = ConfigurationManager.AppSettings["pluginFolder"];

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
        internal ICommand GetCommand(string commandName)
        {
            ICommand cmd = (from c in Commands
                    where c.CommandName.ToLowerInvariant() == commandName.ToLowerInvariant()
                    select c).FirstOrDefault();

            return cmd;
        }
    }
}
