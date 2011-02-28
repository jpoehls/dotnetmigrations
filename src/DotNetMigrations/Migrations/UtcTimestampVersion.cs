using System;
using System.Linq;

namespace DotNetMigrations.Migrations
{
    public class UtcTimestampVersion : IVersionStrategy
    {
        public long GetNewVersionNumber(IMigrationDirectory migrationDirectory)
        {
            var v = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            return v;
        }
    }
}