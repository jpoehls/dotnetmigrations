using System;
using System.Linq;

namespace DotNetMigrations.Migrations
{
    public interface IVersionStrategy
    {
        long GetNewVersionNumber(IMigrationDirectory migrationDirectory);
    }
}