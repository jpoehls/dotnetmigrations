using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Provider;

namespace DotNetMigrations.Repositories
{
    internal class LogRepository : ILogger
    {
        private readonly TypeCatalog catalog;
        private readonly CompositionContainer container;
        private string _logName = "Repository";

        /// <summary>
        /// The name of the log.
        /// </summary>
        public string LogName
        {
            get { return _logName; }
            set { _logName = value; }
        }

        /// <summary>
        /// The log collection used in the application.
        /// </summary>
        [ImportMany("Logs", typeof(ILogger))]
        internal IList<ILogger> Logs { get; set; }

        /// <summary>
        /// Instantiates a new instance of the LogRepository Class.
        /// </summary>
        internal LogRepository()
        {
            Logs = new List<ILogger>();
            catalog = new ConfigurableTypeCatalog("dnm.logs");
            container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        /// <summary>
        /// Retrieves a specific log by name.
        /// </summary>
        /// <param name="logName">The log name.</param>
        /// <returns>An instance of the log.</returns>
        public ILogger GetLog(string logName)
        {
            return (from l in Logs
                    where l.LogName == logName
                    select l).FirstOrDefault();
        }

        /// <summary>
        /// Writes a message to all logs in the collection.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteLine(string message)
        {
            foreach (var log in Logs)
            {
                log.WriteLine(message);
            }
        }

        /// <summary>
        /// Writes a message designated as a warning to all logs in the collection.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteWarning(string message)
        {
            foreach (var log in Logs)
            {
                log.WriteWarning(message);
            }
        }

        /// <summary>
        /// Writes a message designated as a error to all logs in the collection.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteError(string message)
        {
            foreach (var log in Logs)
            {
                log.WriteError(message);
            }
        }

        /// <summary>
        /// Disposes all logs in the collection should they need such.
        /// </summary>
        public void Dispose()
        {
            foreach (var log in Logs)
            {
                log.Dispose();
            }
        }
    }
}