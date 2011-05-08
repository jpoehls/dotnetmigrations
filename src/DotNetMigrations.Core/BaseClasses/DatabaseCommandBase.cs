using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
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
        private readonly ConnectionStringFactory _connectionStringFactory;

        protected DatabaseCommandBase() : this(new ConnectionStringFactory())
        {
            CommandStarting += OnCommandStarting;
            CommandEnded += OnCommandEnded;
        }

        protected DatabaseCommandBase(ConnectionStringFactory connectionStringFactory)
        {
            _connectionStringFactory = connectionStringFactory;
        }

        [Required(ErrorMessage = "-connection is required")]
        [DotConsole.Parameter("connection", Flag = 'c', Position = 0)]
        [Description("Connection string to use, or the name of the connection from app.config to use.")]
        public string Connection { get; set; }

        protected DataAccess Database { get; private set; }

        private void OnCommandStarting(object sender, CommandEventArgs e)
        {
            //  initialize the data access class
            string connStr = GetConnectionString(e.Command as DatabaseCommandBase);
            Database = DataAccessFactory.Create(connStr);

            //  perform the database initialization
            Database.OpenConnection();
            var dbInit = new DatabaseInitializer(Database);
            dbInit.Initialize();
        }

        private string GetConnectionString(DatabaseCommandBase command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            string connStrArg = command.Connection;
            string connStr;
            if (_connectionStringFactory.IsConnectionString(connStrArg))
            {
                connStr = connStrArg;
            }
            else
            {
                connStr = _connectionStringFactory.GetConnectionString(connStrArg);
            }
            return connStr;
        }

        private void OnCommandEnded(object sender, CommandEventArgs e)
        {
            if (Database != null)
            {
                Database.Dispose();
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
                using (DbCommand cmd = Database.CreateCommand())
                {
                    cmd.CommandText = command;
                    var version = cmd.ExecuteScalar<string>();
                    if (version != null)
                    {
                        version = version.Trim();
                    }
                    long.TryParse(version, out currentVersion);

                    if (currentVersion < 0)
                    {
                        throw new SchemaException("schema_migrations table appears to be corrupted");
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