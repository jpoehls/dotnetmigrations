using System;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public static class DbCommandExtensions
    {
        public static T ExecuteScalar<T>(this DbCommand cmd)
        {
            return ExecuteScalar(cmd, default(T));
        }

        public static T ExecuteScalar<T>(this DbCommand cmd, T defaultValue)
        {
            var obj = cmd.ExecuteScalar();
            if (obj == null || obj == DBNull.Value)
            {
                return defaultValue;
            }
            return (T)Convert.ChangeType(obj, typeof (T));            
        }
    }
}