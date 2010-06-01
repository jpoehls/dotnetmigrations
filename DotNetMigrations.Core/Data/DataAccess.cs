using System;
using System.Data.Common;
using System.Linq;

namespace DotNetMigrations.Core.Data
{
    public class DataAccess : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbProviderFactory _factory;

        public DataAccess(DbProviderFactory factory, string connectionString)
        {
            _factory = factory;
            _connection = GetConnection(connectionString);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        #endregion

        public void OpenConnection()
        {
            _connection.Open();
        }

        public void CloseConnection()
        {
            _connection.Close();
        }

        public DbCommand CreateCommand()
        {
            DbCommand cmd = _connection.CreateCommand();
            return cmd;
        }

        public DbTransaction BeginTransaction()
        {
            DbTransaction tran = _connection.BeginTransaction();
            return tran;
        }

        /// <summary>
        /// Creates a connection object based on the Provider specified in the connection string
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>A typed database connection object</returns>
        private DbConnection GetConnection(string connectionString)
        {
            DbConnection conn = _factory.CreateConnection();
            conn.ConnectionString = connectionString;

            return conn;
        }

        /// <summary>
        /// Executes a SQL script. Includes support for executing
        /// scripts in batches using the GO keyword.
        /// </summary>
        public void ExecuteScript(DbTransaction tran, string script)
        {
            var batches = SqlParser.SplitByGoKeyword(script);
            foreach (var batch in batches)
            {
                using (var cmd = CreateCommand())
                {
                    cmd.CommandText = batch;
                    cmd.Transaction = tran;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}