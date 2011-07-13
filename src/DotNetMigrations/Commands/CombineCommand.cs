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

		protected override void Run(CombineCommandArgs args)
		{
			// Migrations that are to be combined
			var scripts = _migrationDirectory.GetScripts()
				.OrderBy(x => x.Version)
				.Where(x => x.Version >= args.StartMigration && x.Version <= args.EndMigration)
				.Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Setup));

			Log.WriteLine(string.Format("{0} migrations to combine.", scripts.Count()));

			// If there are no migrations to combine, short-cut out
			if(!scripts.Any()) return;

			using(StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, args.OutputFile)))
			{
				sw.WriteLine("BEGIN TRANSACTION");
				sw.WriteLine();

				foreach(var script in scripts)
				{
					sw.WriteLine(string.Format(@"/****************************************** {0} ******************************************/", script.Key.Version));
					sw.WriteLine();
					sw.WriteLine(script.Value);
				}

				sw.WriteLine("COMMIT TRANSACTION");
			}

			Log.WriteLine("Combined script saved to: {0}", args.OutputFile);
		}
	}
}
