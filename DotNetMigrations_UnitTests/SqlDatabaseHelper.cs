using System;
using System.Data.SqlClient;
using System.Linq;

namespace DotNetMigrations.UnitTests
{
    public class SqlDatabaseHelper : IDisposable
    {
        private readonly SqlConnection _connection;

        public SqlDatabaseHelper(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _connection.Dispose();
        }

        #endregion

        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                command.Connection = _connection;
                _connection.Open();
                return command.ExecuteNonQuery();
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
            finally
            {
                _connection.Close();
            }
        }
    }
}