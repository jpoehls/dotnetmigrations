using System;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    public class MigrateCommandArgs : DatabaseCommandArguments
    {
        public MigrateCommandArgs()
        {
            TargetVersion = -1;
			transactionmode = MigrationTransactionMode.PerRun.ToString();
        }

        [Argument("version", "v", "Target version to migrate up or down to.",
            Position = 2)]
        public long TargetVersion { get; set; }

		[Argument("transmode", "tm", "The manner in which the migrations should be wrapped in transaction(s).",
			Position = 3)]
		internal string transactionmode { get; set; }
		public MigrationTransactionMode TransactionMode { get { return (MigrationTransactionMode)Enum.Parse(typeof(MigrationTransactionMode), transactionmode, true); } }
    }
}