using System.ComponentModel.Composition;

namespace DotNetMigrations.Core
{
    [InheritedExport("Logs", typeof(ILogger))]
    public abstract class LoggerBase : ILogger
    {
        public abstract string LogName { get; set; }
        
        public abstract void WriteLine(string message);

        public abstract void WriteWarning(string message);

        public abstract void WriteError(string message);

        public virtual void Dispose()
        {
            // do nothing
        }
    }
}
