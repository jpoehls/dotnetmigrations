using System;

namespace DotNetMigrations.Core
{
    /// <summary>
    /// Base class for all DNM commands.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        #region ICommand Members

        public virtual ILogger Log { get; set; }
        public abstract void Execute();

        #endregion

        public void Run()
        {
            if (Log == null)
                throw new InvalidOperationException("ICommand.Log cannot be null.");

            try
            {
                InvokeCommandStarting();
                Execute();
            }
            finally
            {
                InvokeCommandEnded();
            }
        }

        public event EventHandler<CommandEventArgs> CommandStarting;
        public event EventHandler<CommandEventArgs> CommandEnded;

        private void InvokeCommandStarting()
        {
            if (CommandStarting != null)
            {
                CommandStarting(this, new CommandEventArgs(this));
            }
        }

        private void InvokeCommandEnded()
        {
            if (CommandEnded != null)
            {
                CommandEnded(this, new CommandEventArgs(this));
            }
        }
    }
}