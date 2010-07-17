using System;
using System.Configuration;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class ConnectionsCommand : CommandBase<ConnectionsCommandArgs>
    {
        private readonly IConfigurationManager _configManager;

        public ConnectionsCommand()
            : this(new ConfigurationManagerWrapper())
        {
        }

        public ConnectionsCommand(IConfigurationManager configurationManager)
        {
            _configManager = configurationManager;
        }

        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return "connections"; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string Description
        {
            get { return "Allows you to list, add or edit the saved connection strings."; }
        }

        protected override void Run(ConnectionsCommandArgs args)
        {
            //  if no Action was specified, use 'list' as the default
            if (string.IsNullOrEmpty(args.Action))
            {
                args.Action = "list";
            }

            if (string.Equals(args.Action, "list", StringComparison.OrdinalIgnoreCase))
            {
                ListConnectionStrings();
            }
            else if (string.Equals(args.Action, "add", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg(args) || !ValidConnectionStringArg(args))
                    return;

                AddConnectionString(args);
            }
            else if (string.Equals(args.Action, "set", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg(args) || !ValidConnectionStringArg(args))
                    return;

                UpdateConnectionString(args);
            }
            else if (string.Equals(args.Action, "remove", StringComparison.OrdinalIgnoreCase))
            {
                if (!ValidNameArg(args))
                    return;

                RemoveConnectionString(args);
            }
        }

        private void RemoveConnectionString(ConnectionsCommandArgs args)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.ConnectionStrings.ConnectionStrings[args.Name] != null)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(args.Name);
                config.Save(ConfigurationSaveMode.Modified);

                Log.WriteLine("The '{0}' connection string has been removed.", args.Name);
            }
            else
            {
                Log.WriteError("No connection string was found with the name '{0}'.", args.Name);
            }
        }

        private void UpdateConnectionString(ConnectionsCommandArgs args)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.ConnectionStrings.ConnectionStrings[args.Name] != null)
            {
                config.ConnectionStrings.ConnectionStrings[args.Name].ConnectionString = args.ConnectionString;
                config.Save(ConfigurationSaveMode.Modified);

                Log.WriteLine("The '{0}' connection string has been updated.", args.Name);
            }
            else
            {
                Log.WriteError("No connection string was found with the name '{0}'.", args.Name);
            }
        }

        private void AddConnectionString(ConnectionsCommandArgs args)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(args.Name,
                                                                                        args.ConnectionString));
            config.Save(ConfigurationSaveMode.Modified);

            Log.WriteLine("The '{0}' connection string has been added.", args.Name);
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

        private bool ValidConnectionStringArg(ConnectionsCommandArgs args)
        {
            if (string.IsNullOrEmpty(args.ConnectionString))
            {
                Log.WriteError("Invalid arguments:");
                Log.WriteError("* -connectionString is required");
                return false;
            }
            return true;
        }

        private bool ValidNameArg(ConnectionsCommandArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                Log.WriteError("Invalid arguments:");
                Log.WriteError("* -name is required");
                return false;
            }
            return true;
        }
    }
}