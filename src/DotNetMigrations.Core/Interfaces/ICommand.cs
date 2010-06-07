using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotNetMigrations.Core
{
    [InheritedExport("Commands", typeof (ICommand))]
    public interface ICommand
    {
        string CommandName { get; }
        string Description { get; }
        ILogger Log { get; set; }

        void Run(IArguments args);
        IArguments CreateArguments();
        Type GetArgumentsType();
    }
}