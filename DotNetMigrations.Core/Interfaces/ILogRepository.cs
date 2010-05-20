using System;
using System.Collections.Generic;

namespace DotNetMigrations.Core
{
    public interface ILogRepository : IDisposable
    {
        string LogName { get; set; }
        IList<ILogger> Logs { get; set; }
        void WriteLine(string message);
        void WriteWarning(string message);
        void WriteError(string message);
    }
}
