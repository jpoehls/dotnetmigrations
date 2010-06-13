using System;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public static class DbCommandExtensions
    {
        public static T ExecuteScalar<T>(this DbCommand cmd)
        {
            var obj = cmd.ExecuteScalar();
            if (obj == DBNull.Value)
            {
                return default(T);
            }
            return (T) obj;
        }
    }
}