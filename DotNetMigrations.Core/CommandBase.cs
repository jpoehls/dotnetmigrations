using System;
using System.Collections.Generic;

namespace DotNetMigrations.Core
{
    /// <summary>
    /// Base class for all DNM commands.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        protected abstract void RunCommand();
        protected abstract bool ValidateArguments();

        public abstract string CommandName { get; }
        public abstract string HelpText { get; }

        public IArgumentRepository Arguments { get; set; }
        public ILogger Log { get; set; }

        public virtual IList<ICommand> SubCommands { get; set; }

        public event EventHandler CommandEnded;

        public CommandResults Run()
        {
            if (!ValidateArguments())
            {
                InvokeCommandEnded();
                return CommandResults.Invalid;
            }

            RunCommand();

            InvokeCommandEnded();
            return CommandResults.Success;
        }

        private void InvokeCommandEnded()
        {
            if (CommandEnded != null)
            {
                CommandEnded(this, EventArgs.Empty);
            }
        }
    }
}
