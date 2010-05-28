using System;
using System.Data.Common;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Core
{
    /// <summary>
    /// Base class for commands that will interact with the database.
    /// 
    /// Using this base class adds the requirement that the 2nd
    /// command argument must always be the connection string.
    /// </summary>
    public abstract class DatabaseCommandBase : CommandBase
    {
        protected DataAccess DataAccess { get; private set; }

        protected override bool ValidateArguments()
        {
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number arguments is too few.");
                return false;
            }

            return true;
        }

        protected override void RunCommand()
        {
            CommandEnded += OnCommandEnded;

            //  initialize the data access class
            string connStr = GetConnectionString();
            DataAccess = DataAccess.Open(connStr);

            //  perform the database initialization
            var dbInit = new DatabaseInitializer(DataAccess);
            dbInit.Initialize();
        }

        private string GetConnectionString()
        {
            string connStrArg = Arguments.GetArgument(1);
            string connStr;
            if (ConnectionStringFactory.IsConnectionString(connStrArg))
            {
                connStr = connStrArg;
            }
            else
            {
                connStr = ConnectionStringFactory.GetConnectionString(connStrArg);
            }
            return connStr;
        }

        private void OnCommandEnded(object sender, EventArgs e)
        {
            if (DataAccess != null)
            {
                DataAccess.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <returns>The current version of the database.</returns>
        protected long GetDatabaseVersion()
        {
            const string command = "SELECT MAX([version]) FROM [schema_migrations]";

            long currentVersion = -1;

            try
            {
                using (var cmd = DataAccess.CreateCommand())
                {
                    cmd.CommandText = command;
                    var version = cmd.ExecuteScalar<string>();
                    if (version != null)
                    {
                        version = version.Trim();
                    }
                    long.TryParse(version, out currentVersion);

                    if (currentVersion != -1)
                    {
                        Log.WriteLine("Current Database Version:".PadRight(30) + version);
                    }
                }
            }
            catch (DbException ex)
            {
                Log.WriteError(ex.Message);
            }

            return currentVersion;
        }
    }
}