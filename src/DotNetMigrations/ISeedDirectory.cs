using System.Collections.Generic;
using DotNetMigrations.Core;

namespace DotNetMigrations
{
    public interface ISeedDirectory
    {
        /// <summary>
        /// Returns the seed script path from the
        /// config file (if available) or the default path.
        /// </summary>
        string GetPath(ILogger log);

        /// <summary>
        /// Returns a list of all the seed script file paths sorted by name.
        /// </summary>
        IEnumerable<string> GetScripts(string setName);
    }
}