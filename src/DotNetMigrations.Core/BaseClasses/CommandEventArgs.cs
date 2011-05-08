using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    public class CommandEventArgs : EventArgs
    {
        public ICommand Command { get; private set; }

        public CommandEventArgs(ICommand command)
        {
            Command = command;
        }
    }
}