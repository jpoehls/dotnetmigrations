using System;
using System.Linq;

namespace DotNetMigrations.Migrations
{
    public class LocalTimestampVersion : IVersionStrategy
    {
        public long GetNewVersionNumber()
        {
            var v = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss").ToString());
            return v;
        }
    }
}