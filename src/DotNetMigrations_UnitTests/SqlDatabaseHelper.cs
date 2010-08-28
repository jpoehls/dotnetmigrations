using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.UnitTests
{
    public class SqlDatabaseHelper : IDisposable
    {
        private readonly SqlCeConnection _connection;

        public SqlDatabaseHelper(string connectionString)
        {
            _connection = new SqlCeConnection(GetConnectionStringWithoutProvider(connectionString));
        }

        public static string GetConnectionStringWithoutProvider(string connStr)
        {
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connStr;
            if (builder.ContainsKey("provider"))
                builder.Remove("provider");
            return builder.ConnectionString;
        }

        public bool SwallowSqlExceptions { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _connection.Dispose();
        }

        #endregion

        public T ExecuteScalar<T>(string commandText)
        {
            return ExecuteScalar(commandText, default(T));
        }

        public T ExecuteScalar<T>(string commandText, T defaultValue)
        {
            var cmd = new SqlCeCommand(commandText);
            var obj = ExecuteScalar(cmd);
            if (obj == DBNull.Value)
            {
                return defaultValue;
            }
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        private object ExecuteScalar(SqlCeCommand command)
        {
            try
            {
                command.Connection = _connection;
                _connection.Open();
                return command.ExecuteScalar();
            }
            catch (SqlCeException)
            {
                if (!SwallowSqlExceptions)
                {
                    throw;
                }
                return null;
            }
            finally
            {
                _connection.Close();
            }
        }

        public int ExecuteNonQuery(SqlCeCommand command)
        {
            try
            {
                command.Connection = _connection;
                _connection.Open();
                return command.ExecuteNonQuery();
            }
            catch (SqlCeException)
            {
                if (!SwallowSqlExceptions)
                {
                    throw;
                }
                return 0;
            }
            finally
            {
                _connection.Close();
            }
        }

        public int ExecuteNonQuery(string commandText)
        {
            var cmd = new SqlCeCommand(commandText);
            return ExecuteNonQuery(cmd);
        }

        public void ExecuteNonQuery(params string[] commandTexts)
        {
            ExecuteNonQuery(commandTexts.Select(x => new SqlCeCommand(x)).ToArray());
        }

        public void ExecuteNonQuery(params SqlCeCommand[] commands)
        {
            try
            {
                _connection.Open();
                foreach (SqlCeCommand cmd in commands)
                {
                    cmd.Connection = _connection;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlCeException)
            {
                if (!SwallowSqlExceptions)
                {
                    throw;
                }
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}