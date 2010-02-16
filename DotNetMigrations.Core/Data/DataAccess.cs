using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public class DataAccess
    {
        #region ConnectionString

        /// <summary>
        /// Retrieves the connection string based on the supplied Migration Name
        /// </summary>
        /// <param name="migrationName">The name of the migration name key.</param>
        /// <returns>The connection string associated to the migration name.</returns>
        public string GetConnectionString(string migrationName)
        {
            return GetConnectionString(migrationName, null);
        }

        /// <summary>
        /// Retrieves the connection string based on the supplied Migration Name or connection string.
        /// </summary>
        /// <param name="migrationName">The name of the migration name key.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The connection string associated to the migration name.</returns>
        public string GetConnectionString(string migrationName, string connectionString)
        {
            string returnString;

            if(!string.IsNullOrEmpty(connectionString))
            {
                returnString = connectionString;
            }
            else
            {
                returnString = ConfigurationManager.ConnectionStrings[migrationName].ConnectionString;
            }

            return returnString;
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Executes a sql scalar command against a database and casting it to the speified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the results into.</typeparam>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="commandString">The sql command to use.</param>
        /// <returns>The value from the database.</returns>
        public T ExecuteScalar<T>(string connectionString, string commandString)
        {
            return (T)ExecuteScalar(connectionString, commandString, false);
        }

        /// <summary>
        /// Executes a sql scalar command against a database and casting it to the speified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the results into.</typeparam>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="commandString">The sql command to use.</param>
        /// <param name="suppressErrors">Specifies whether or not sql errors should be suppressed.</param>
        /// <returns>The value from the database.</returns>
        public T ExecuteScalar<T>(string connectionString, string commandString, bool suppressErrors)
        {
            return (T)ExecuteScalar(connectionString, commandString, suppressErrors);
        }

        /// <summary>
        /// Executes a sql scalar command against a database
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="commandString">The sql command to use.</param>
        /// <returns>The value from the database.</returns>
        public object ExecuteScalar(string connectionString, string commandString)
        {
            return ExecuteScalar(connectionString, commandString, false);
        }

        /// <summary>
        /// Executes a sql scalar command against a database
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="commandString">The sql command to use.</param>
        /// <param name="suppressErrors">Specifies whether or not sql errors should be suppressed.</param>
        /// <returns>The value from the database.</returns>
        public object ExecuteScalar(string connectionString, string commandString, bool suppressErrors)
        {
            object returnValue = null;

            try
            {
                using (DbConnection conn = GetConnection(connectionString))
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = commandString;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    returnValue = cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                if (!suppressErrors)
                {
                    throw;
                }
            }

            if (returnValue is DBNull)
            {
                returnValue = string.Empty;
            }

            return returnValue;
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes a query that returns no value from the database.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="commandString">The sql command to use.</param>
        public void ExecuteNonQuery(string connectionString, string commandString)
        {
            using (DbConnection conn = GetConnection(connectionString))
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = commandString;
                cmd.CommandType = CommandType.Text;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        /// <summary>
        /// Creates a connection object based on the Provider specified in the connection string
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A typed database connection object</returns>
        private DbConnection GetConnection(string connectionString)
        {
            DbConnectionStringBuilder csb = new DbConnectionStringBuilder();
            csb.ConnectionString = connectionString;

            string provider = GetProvider(csb);

            DbConnection conn = GetFactory(provider).CreateConnection();
            conn.ConnectionString = csb.ToString();

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
            string provider = "System.Data.SqlClient";	// default factory provider
            
            if (csb.ContainsKey("provider"))
            {
                provider = csb["provider"].ToString();
                csb.Remove("provider");
            }

            return provider;
        }
    }
}
