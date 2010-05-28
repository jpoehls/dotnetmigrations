using System;
using System.Configuration;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public class ConnectionStringFactory
    {
        /// <summary>
        /// Gets the named connection string from app.Config
        /// </summary>
        public static string GetConnectionString(string name)
        {
            var connStr = ConfigurationManager.ConnectionStrings[name];
            if (connStr == null)
                throw new ArgumentException("No connection string was found with the name \"" + name + "\"", "name");

            return connStr.ConnectionString;
        }

        /// <summary>
        /// Returns true/false whether the given value looks like a connection string.
        /// </summary>
        public static bool IsConnectionString(string val)
        {
            var csb = new DbConnectionStringBuilder();
            csb.ConnectionString = val;
            return csb.Count > 0;
        }
    }
}