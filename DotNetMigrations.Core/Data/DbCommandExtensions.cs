using System;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public static class DbCommandExtensions
    {
        public static T ExecuteScalar<T>(this DbCommand cmd)
        {
            return (T)cmd.ExecuteScalar();
        }
    }
}