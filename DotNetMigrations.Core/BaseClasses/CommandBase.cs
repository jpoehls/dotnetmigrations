using System.Collections.Generic;

namespace DotNetMigrations.Core
{
    public abstract class CommandBase : ICommand
    {
        protected abstract void RunCommand();
        protected abstract bool ValidateArguments();

        public abstract string CommandName { get; }
        public abstract string HelpText { get; }

        public IArgumentRepository Arguments { get; set; }
        public ILogger Log { get; set; }

        public virtual IList<ICommand> SubCommands { get; set; }

        public CommandResults Run()
        {
            if (!ValidateArguments())
            {
                return CommandResults.Invalid;
            }

            RunCommand();

            return CommandResults.Success;
        }
    }
}
