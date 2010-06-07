using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    public class CommandEventArgs<TArgs> : EventArgs
        where TArgs : CommandArguments
    {
        public TArgs CommandArguments { get; private set; }

        public CommandEventArgs(TArgs commandArguments)
        {
            CommandArguments = commandArguments;
        }
    }
}