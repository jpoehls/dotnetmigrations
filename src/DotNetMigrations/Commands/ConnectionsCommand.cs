using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using DotConsole;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    [Command("connections")]
    [Description("Allows you to list, add or edit the saved connection strings.")]
    internal class ConnectionsCommand : CommandBase
    {
        private readonly IConfigurationManager _configManager;

        [ValueSetValidator("", "list", "add", "set", "remove", ErrorMessage = "-action must be 'list', 'add', 'set', or 'remove'")]
        [Parameter("action", Flag='a', Position = 0)]
        [Description("Action to perform. [ list | add | set | remove ]")]
        public string Action { get; set; }

        [Parameter("name", Flag='n', Position = 1)]
        [Description("Name of connection to 'add', 'set' or 'remove'.")]
        public string Name { get; set; }

        [Parameter("connectionString", Flag='C', Position = 2)]
        [Description("Connection string to 'add' or 'set'.")]
        public string ConnectionString { get; set; }

        public ConnectionsCommand()
            : this(new ConfigurationManagerWrapper())
        {
        }

        public ConnectionsCommand(IConfigurationManager configurationManager)
        {
            _configManager = configurationManager;
        }

        public override void Execute()
        {
            //  if no Action was specified, use 'list' as the default
            if (string.IsNullOrEmpty(Action))
            {
                Action = "list";
            }

            if (string.Equals(Action, "list", StringComparison.OrdinalIgnoreCase))
            {
                ListConnectionStrings();
            }
            else if (string.Equals(Action, "add", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg() || !ValidConnectionStringArg())
                    return;

                AddConnectionString();
            }
            else if (string.Equals(Action, "set", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg() || !ValidConnectionStringArg())
                    return;

                UpdateConnectionString();
            }
            else if (string.Equals(Action, "remove", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg())
                    return;

                RemoveConnectionString();
            }
        }

        private void RemoveConnectionString()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.ConnectionStrings.ConnectionStrings[Name] != null)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(Name);
                config.Save(ConfigurationSaveMode.Modified);

                Log.WriteLine("The '{0}' connection string has been removed.", Name);
            }
            else
            {
                Log.WriteError("No connection string was found with the name '{0}'.", Name);
            }
        }

        private void UpdateConnectionString()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.ConnectionStrings.ConnectionStrings[Name] != null)
            {
                config.ConnectionStrings.ConnectionStrings[Name].ConnectionString = ConnectionString;
                config.Save(ConfigurationSaveMode.Modified);

                Log.WriteLine("The '{0}' connection string has been updated.", Name);
            }
            else
            {
                Log.WriteError("No connection string was found with the name '{0}'.", Name);
            }
        }

        private void AddConnectionString()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(Name,
                                                                                        ConnectionString));
            config.Save(ConfigurationSaveMode.Modified);

            Log.WriteLine("The '{0}' connection string has been added.", Name);
        }

        private void ListConnectionStrings()
        {
            int maxNameLength = 0;

            foreach (ConnectionStringSettings conn in _configManager.ConnectionStrings)
            {
                maxNameLength = Math.Max(maxNameLength, conn.Name.Length);
            }

            foreach (ConnectionStringSettings conn in _configManager.ConnectionStrings)
            {
                Log.Write((conn.Name + ": ").PadRight(maxNameLength + 4));
                Log.WriteLine(conn.ConnectionString);
            }
        }

        private bool ValidConnectionStringArg()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                Log.WriteError("Invalid arguments:");
                Log.WriteError("* -connectionString is required");
                return false;
            }
            return true;
        }

        private bool ValidNameArg()
        {
            if (string.IsNullOrEmpty(Name))
            {
                Log.WriteError("Invalid arguments:");
                Log.WriteError("* -name is required");
                return false;
            }
            return true;
        }
    }
}