using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;
using System.Data.Common;
using System.IO;

namespace DotNetMigrations.Commands
{
	internal class CombineCommand : CommandBase<CombineCommandArgs>
	{
		private readonly IMigrationDirectory _migrationDirectory;

		public CombineCommand()
			: this(new MigrationDirectory())
		{
		}

		public CombineCommand(IMigrationDirectory migrationDirectory)
		{
			_migrationDirectory = migrationDirectory;
		}

		public override string CommandName
		{
			get { return "combine"; }
		}

		public override string Description
		{
			get { return "Combines a range of migrations into a single upgrade script."; }
		}

		protected override void Execute(CombineCommandArgs args)
		{
			var allscripts = _migrationDirectory.GetScripts()
				.OrderBy(x => x.Version);

			// Special case if no "end migration" specified, use the most recent script
			if(args.EndMigration == long.MaxValue) args.EndMigration = allscripts.Last().Version;

			// Check that the start and end version scripts actually exist
			if(!allscripts.Any(x => x.Version == args.StartMigration))
			{
				Log.WriteError("Start version migration " + args.StartMigration + " could not be found.");
				return;
			}
			if(!allscripts.Any(x => x.Version == args.EndMigration))
			{
				Log.WriteError("End version migration " + args.StartMigration + " could not be found.");
				return;
			}

			// Migrations that are to be combined
			var scripts = allscripts
				.Where(x => x.Version >= args.StartMigration && x.Version <= args.EndMigration)
				.Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Setup));

			Log.WriteLine("Transaction mode is: " + args.TransactionMode.ToString() + ".");
			Log.WriteLine("");

			Log.WriteLine(string.Format("{0} migrations to combine.", scripts.Count()));

			// If there are no migrations to combine, short-cut out
			if(!scripts.Any()) return;

			using(StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, args.OutputFile)))
			{
				sw.WriteLine(string.Format(
@"/*
		{0} to {1} 

		({2}) 
*/

DECLARE @startmigration BIGINT, @endmigration BIGINT, @dbversion BIGINT 
SELECT @startmigration = {0}
SELECT @endmigration = {1}
SELECT @dbversion = MAX([version]) FROM [schema_migrations]

PRINT 'Transaction mode is: {3}.'
PRINT ''
PRINT 'Database is at version:  ' + CONVERT(NVARCHAR, @dbversion)
PRINT 'Target version:          ' + CONVERT(NVARCHAR, @endmigration)
PRINT ''

IF @dbversion >= @startmigration 
BEGIN
	RAISERROR('The migrations contained in this script have already been run.', 20, -1) WITH LOG
END
GO
", args.StartMigration, args.EndMigration, DateTime.Now.ToString(), args.TransactionMode.ToString()));

				if(args.TransactionMode == MigrationTransactionMode.PerRun)
				{
					sw.WriteLine("BEGIN TRANSACTION");
					sw.WriteLine("GO");
					sw.WriteLine();
				}

				foreach(var script in scripts)
				{
					Log.WriteLine(script.Key.FilePath);

					sw.WriteLine(string.Format("/****************************************** {0} ******************************************/", script.Key.Version));
					sw.WriteLine();
					sw.WriteLine(string.Format("PRINT 'EXECUTING {0}' ", script.Key.Version));

					if(args.TransactionMode == MigrationTransactionMode.PerMigration)
					{
						sw.WriteLine("BEGIN TRANSACTION");
						sw.WriteLine("GO");
						sw.WriteLine();
					}

					sw.WriteLine("/* MIGRATION START */");
					sw.WriteLine(script.Value);
					sw.WriteLine("/* MIGRATION END */");

					// Update "version"
					sw.WriteLine();
					sw.WriteLine(string.Format("INSERT INTO [schema_migrations] ([version]) VALUES ({0})", script.Key.Version));

					if(args.TransactionMode == MigrationTransactionMode.PerMigration)
					{
						sw.WriteLine("COMMIT TRANSACTION");
						sw.WriteLine("GO");
						sw.WriteLine();
					}
				}

				if(args.TransactionMode == MigrationTransactionMode.PerRun)
				{
					sw.WriteLine("COMMIT TRANSACTION");
					sw.WriteLine("GO");
					sw.WriteLine();
				}
			}

			Log.WriteLine("");
			Log.WriteLine("Combined script saved to: {0}", args.OutputFile);
		}
	}
}
