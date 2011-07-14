using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    /// <summary>
    /// Base class for all DNM commands.
    /// </summary>
    public abstract class CommandBase<TArgs> : ICommand
        where TArgs : CommandArguments, new()
    {
        #region ICommand Members

        public abstract string CommandName { get; }
        public abstract string Description { get; }
        public virtual ILogger Log { get; set; }

        public Type GetArgumentsType()
        {
            return typeof (TArgs);
        }

        public IArguments CreateArguments()
        {
            return new TArgs();
        }

        public void Run(IArguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (!(args is TArgs))
                throw new ArgumentException("args type doesn't match generic type", "args");

            if (Log == null)
                throw new InvalidOperationException("ICommand.Log cannot be null.");

            if (!args.IsValid)
                throw new InvalidOperationException("Argument validation failed. Arguments are invalid.");

            var commandArgs = (TArgs) args;

            try
            {
                InvokeCommandStarting(commandArgs);
                Execute(commandArgs);
            }
            finally
            {
                InvokeCommandEnded(commandArgs);
            }
        }

        #endregion

        protected abstract void Execute(TArgs args);
        public event EventHandler<CommandEventArgs<TArgs>> CommandStarting;
        public event EventHandler<CommandEventArgs<TArgs>> CommandEnded;

        private void InvokeCommandStarting(TArgs commandArgs)
        {
            if (CommandStarting != null)
            {
                CommandStarting(this, new CommandEventArgs<TArgs>(commandArgs));
            }
        }

        private void InvokeCommandEnded(TArgs commandArgs)
        {
            if (CommandEnded != null)
            {
                CommandEnded(this, new CommandEventArgs<TArgs>(commandArgs));
            }
        }
    }
}