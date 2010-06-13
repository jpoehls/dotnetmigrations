using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.UnitTests
{
    public class SqlDatabaseHelper : IDisposable
    {
        private readonly SqlConnection _connection;

        public SqlDatabaseHelper(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public bool SwallowSqlExceptions { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _connection.Dispose();
        }

        #endregion

        public object ExecuteScalar(string commandText)
        {
            var cmd = new SqlCommand(commandText);
            return ExecuteScalar(cmd);
        }

        public object ExecuteScalar(SqlCommand command)
        {
            try
            {
                command.Connection = _connection;
                _connection.Open();
                return command.ExecuteScalar();
            }
            catch (SqlException)
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

        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                command.Connection = _connection;
                _connection.Open();
                return command.ExecuteNonQuery();
            }
            catch (SqlException)
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
            var cmd = new SqlCommand(commandText);
            return ExecuteNonQuery(cmd);
        }

        public void ExecuteNonQuery(params string[] commandTexts)
        {
            ExecuteNonQuery(commandTexts.Select(x => new SqlCommand(x)).ToArray());
        }

        public void ExecuteNonQuery(params SqlCommand[] commands)
        {
            try
            {
                _connection.Open();
                foreach (SqlCommand cmd in commands)
                {
                    cmd.Connection = _connection;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
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

        public void DropAllObjects()
        {
            var assm = Assembly.GetExecutingAssembly();
            using (var s = assm.GetManifestResourceStream("DotNetMigrations.UnitTests.Resources.drop_all_objects.sql"))
            {
                using (var reader = new StreamReader(s))
                {
                    var scripts = SqlParser.SplitByGoKeyword(reader.ReadToEnd());
                    ExecuteNonQuery(scripts.ToArray());
                }
            }
        }
    }
}