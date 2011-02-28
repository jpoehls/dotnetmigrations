using System.Linq;

namespace DotNetMigrations.Migrations
{
    public class SequentialNumberVersion : IVersionStrategy
    {
        public long GetNewVersionNumber(IMigrationDirectory migrationDirectory)
        {
            // get the latest number in use
            var lastNumber = migrationDirectory.GetScripts().Max(script => script.Version);

            // increment and return
            return lastNumber + 1;
        }
    }
}