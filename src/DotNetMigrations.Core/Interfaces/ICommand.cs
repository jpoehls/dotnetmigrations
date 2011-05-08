using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotNetMigrations.Core
{
    [InheritedExport("Commands", typeof(ICommand))]
    public interface ICommand : DotConsole.ICommand
    {
        ILogger Log { get; set; }
    }
}