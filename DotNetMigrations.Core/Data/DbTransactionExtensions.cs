using System;
using System.Data.Common;

namespace DotNetMigrations.Core.Data
{
    public static class DbTransactionExtensions
    {
        public static DbCommand CreateCommand(this DbTransaction transaction)
        {
            var cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return cmd;
        }
    }
}