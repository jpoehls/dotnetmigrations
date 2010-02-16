using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DotNetMigrations.Core
{
    [InheritedExport("Commands", typeof(ICommand))]
    public interface ICommand
    {
        string CommandName { get; set; }
        string HelpText { get; set; }
        IList<ICommand> SubCommands { get; set; }
        ILogger Log { get; set; }
        IArgumentRepository Arguments { get; set; }

        CommandResults Run();
    }
}
