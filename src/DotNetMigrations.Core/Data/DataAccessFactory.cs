using System;
using System.Data.Common;
using System.Linq;

namespace DotNetMigrations.Core.Data
{
    public class DataAccessFactory
    {
        /// <summary>
        /// Creates and returns a new instance for the given connection string.
        /// </summary>
        public static DataAccess Create(string connectionString)
        {
            var csb = new DbConnectionStringBuilder();
            csb.ConnectionString = connectionString;
            
            string provider = GetProvider(csb);
            int commandTimeout = GetCommandTimeout(csb);
            
            var da = new DataAccess(GetFactory(provider), csb.ConnectionString, provider, commandTimeout);
            return da;
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
            const string key = "provider";
            string provider = "System.Data.SqlClient"; // default factory provider

            if (csb.ContainsKey(key))
            {
                provider = csb[key].ToString();
                csb.Remove(key);
            }

            return provider;
        }

        /// <summary>
        /// Gets the command timeout (in seconds) from
        /// the connection string (if the CommandTimeout key is specified),
        /// or a default value of 30 seconds.
        /// </summary>
        private static int GetCommandTimeout(DbConnectionStringBuilder csb)
        {
            const string key = "CommandTimeout";
            const int defaultValue = 30;

            int value = defaultValue;
            if (csb.ContainsKey(key))
            {
                if (Int32.TryParse(csb[key].ToString(), out value))
                {
                    // guard against negative values
                    // uses the default timeout value if a negative is given
                    value = Math.Max(defaultValue, value);
                }

                csb.Remove(key);
            }

            return value;
        }
    }
}