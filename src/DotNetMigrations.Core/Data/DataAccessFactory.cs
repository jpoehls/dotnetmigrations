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

            var da = new DataAccess(GetFactory(provider), csb.ConnectionString, provider);
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