using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMigrations.Core;
using System.ComponentModel.DataAnnotations;

namespace DotNetMigrations.Commands
{
	public class CombineCommandArgs : CommandArguments
	{
		public CombineCommandArgs()
        {
			this.EndMigration = Int64.MaxValue;
			transactionmode = MigrationTransactionMode.PerRun.ToString();
        }

		[Required(ErrorMessage = "-start is required")]
		[Argument("start", "s", "First migration (oldest) to combine.", Position = 1)]
		public long StartMigration { get; set; }

		[Required(ErrorMessage = "-end is required")]
		[Argument("end", "e", "Last migration (newest) to combine.", Position = 2)]
		public long EndMigration { get; set; }

		[Required(ErrorMessage = "-out is required")]
		[Argument("out", "out", "Output file for the full script.", Position = 3)]
		public string OutputFile { get; set; }

		[Argument("transmode", "tm", "The manner in which the migrations should be wrapped in transaction(s).", Position = 4)]
		public string transactionmode { get; set; }
		public MigrationTransactionMode TransactionMode { get { return (MigrationTransactionMode)Enum.Parse(typeof(MigrationTransactionMode), transactionmode, true); } }
	}
}
