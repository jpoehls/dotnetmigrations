using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    public interface IPostMigrationHook
    {
        string CommandName { get; }
        ILogger Log { get; set; }
        void OnPostMigration(DatabaseCommandArguments args);
    }
}
