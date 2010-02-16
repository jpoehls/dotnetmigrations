using System;

namespace DotNetMigrations.Core
{
    public interface ILogger : IDisposable
    {
        string LogName { get; set; }
        void WriteLine(string message);
        void WriteWarning(string message);
        void WriteError(string message);
    }
}
