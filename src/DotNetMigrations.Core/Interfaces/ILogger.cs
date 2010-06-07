using System;

namespace DotNetMigrations.Core
{
    public interface ILogger : IDisposable
    {
        string LogName { get; set; }
        
        void Write(string message);
        void Write(string format, params object[] args);

        void WriteLine(string message);
        void WriteLine(string format, params object[] args);

        void WriteWarning(string message);
        void WriteWarning(string format, params object[] args);

        void WriteError(string message);
        void WriteError(string format, params object[] args);
    }
}
