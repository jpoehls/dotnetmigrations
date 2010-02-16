using System.Collections.Generic;

namespace DotNetMigrations.Core
{
    public interface IArgumentRepository
    {
        List<string> Arguments { get; set; }
        bool HasArgument(string arg);
        string GetArgument(int index);
        string GetLastArgument();
        int Count { get; }
    }
}
