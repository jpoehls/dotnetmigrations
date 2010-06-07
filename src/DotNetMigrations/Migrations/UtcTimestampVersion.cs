using System;
using System.Linq;

namespace DotNetMigrations.Migrations
{
    public class UtcTimestampVersion : IVersionStrategy
    {
        public long GetNewVersionNumber()
        {
            var v = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss").ToString());
            return v;
        }
    }
}