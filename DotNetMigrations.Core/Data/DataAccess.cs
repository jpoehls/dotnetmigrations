using System;
using System.Data.Common;
using System.Linq;

namespace DotNetMigrations.Core.Data
{
    public class DataAccess : IDisposable
    {
        private DbConnection _connection;
        private DbProviderFactory _factory;

        /// <remarks>
        /// Constructor is private to force use of the factory method.
        /// 
        /// Prefer the factory method over the constructor here
        /// because constructors should never throw exceptions
        /// and the connection instantiation could throw.
        /// </remarks>
        private DataAccess()
        {
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        #endregion

        /// <summary>
        /// Creates and returns a new instance for the given connection string.
        /// Also opens a connection to the database.
        /// </summary>
        public static DataAccess Open(string connectionString)
        {
            var csb = new DbConnectionStringBuilder();
            csb.ConnectionString = connectionString;

            string provider = GetProvider(csb);

            var da = new DataAccess();
            da._factory = GetFactory(provider);
            da._connection = da.GetConnection(connectionString);
            return da;
        }

        public DbCommand CreateCommand()
        {
            DbCommand cmd = _connection.CreateCommand();
            return cmd;
        }

        public DbTransaction BeginTransaction()
        {
            DbTransaction tran = _connection.BeginTransaction();
            return tran;
        }

        /// <summary>
        /// Creates a connection object based on the Provider specified in the connection string
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A typed database connection object</returns>
        private DbConnection GetConnection(string connectionString)
        {
            DbConnection conn = _factory.CreateConnection();
            conn.ConnectionString = connectionString;

            return conn;
        }

        /// <summary>
        /// Retrieves the appropriate Database provider factory.
        /// </summary>
        /// <param name="provider">The string type of the provider whose factory to create</param>
        /// <returns>An instance of the database provider factory.</returns>
        private static DbProviderFactory GetFactory(string provider)
        {
            return DbProviderFactories.GetFactory(provider);
        }

        /// <summary>
        /// Extracts the provider from the connection string or uses the default.
        /// </summary>
        /// <param name="csb">The DbConnectionStringBuilder object to use.</param>
        /// <returns>the string type of the provider.</returns>
        private static string GetProvider(DbConnectionStringBuilder csb)
        {
            string provider = "System.Data.SqlClient"; // default factory provider

            if (csb.ContainsKey("provider"))
            {
                provider = csb["provider"].ToString();
                csb.Remove("provider");
            }

            return provider;
        }
    }
}