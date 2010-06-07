using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotNetMigrations.Core
{
    [InheritedExport("Logs", typeof (ILogger))]
    public abstract class LoggerBase : ILogger
    {
        #region ILogger Members

        public abstract string LogName { get; set; }

        public abstract void Write(string message);

        public void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        public abstract void WriteLine(string message);

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public abstract void WriteWarning(string message);

        public void WriteWarning(string format, params object[] args)
        {
            WriteWarning(string.Format(format, args));
        }

        public abstract void WriteError(string message);

        public void WriteError(string format, params object[] args)
        {
            WriteError(string.Format(format, args));
        }

        public virtual void Dispose()
        {
            // do nothing
        }

        #endregion
    }
}