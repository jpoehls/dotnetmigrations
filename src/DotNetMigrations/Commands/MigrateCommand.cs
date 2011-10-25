using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
	public class MigrateCommand : DatabaseCommandBase<MigrateCommandArgs>
	{
		private readonly IMigrationDirectory _migrationDirectory;

		public MigrateCommand()
			: this(new MigrationDirectory())
		{
		}

		public MigrateCommand(IMigrationDirectory migrationDirectory)
		{
			_migrationDirectory = migrationDirectory;
		}

		/// <summary>
		/// The name of the command that is typed as a command line argument.
		/// </summary>
		public override string CommandName
		{
			get { return "migrate"; }
		}

		/// <summary>
		/// The help text information for the command.
		/// </summary>
		public override string Description
		{
			get { return "Migrates the database up or down to a specific version."; }
		}

		/// <summary>
		/// Executes the Command's logic.
		/// </summary>
		protected override void Execute(MigrateCommandArgs args)
		{
			_migrationDirectory.Path = args.MigrationsPath;

			IOrderedEnumerable<IMigrationScriptFile> files = _migrationDirectory.GetScripts()
				.OrderByDescending(x => x.Version);

			if (files.Count() == 0)
			{
				Log.WriteLine("No migration scripts were found.");
				return;
			}

			long currentVersion = GetDatabaseVersion();
			long targetVersion = args.TargetVersion;

			if (targetVersion == -1)
			{
				//  if version not provided, assume we want to migrate to the latest migration script version
				targetVersion = files.Select(x => x.Version).First();
			}

			Log.WriteLine("Database is at version:".PadRight(30) + currentVersion);

			MigrationDirection direction;
			if (currentVersion < targetVersion)
			{
				direction = MigrationDirection.Up;
				MigrateUp(currentVersion, targetVersion, files);
				Log.WriteLine("Migrated up to version:".PadRight(30) + targetVersion);
			}
			else if (currentVersion > targetVersion)
			{
				direction = MigrationDirection.Down;
				MigrateDown(currentVersion, targetVersion, files);
				Log.WriteLine("Migrated down to version:".PadRight(30) + targetVersion);
			}
			else
			{
				return;
			}

			// execute the post migration actions
			var postMigrationHooks = Program.Current.CommandRepository.Commands
				.Where(cmd => cmd is IPostMigrationHook)
				.Cast<IPostMigrationHook>()
				.Where(hook => hook.ShouldRun(direction));

			if (postMigrationHooks.Count() > 0)
			{
				Log.WriteLine("Executing post migration hooks...");

				foreach (var hook in postMigrationHooks)
				{
					Log.WriteLine("  {0}", hook.CommandName);
					hook.Log = Log;
					hook.OnPostMigration(args, direction);
				}
			}
			else
			{
				Log.WriteLine("No post migration hooks were run.");
			}
		}

		/// <summary>
		/// Migrates the database up to the targeted version.
		/// </summary>
		/// <param name="currentVersion">The current version of the database.</param>
		/// <param name="targetVersion">The targeted version of the database.</param>
		/// <param name="files">All migration script files.</param>
		private void MigrateUp(long currentVersion, long targetVersion, IEnumerable<IMigrationScriptFile> files)
		{
			IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts = files.OrderBy(x => x.Version)
				.Where(x => x.Version > currentVersion && x.Version <= targetVersion)
				.Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Setup));

			ExecuteMigrationScripts(scripts, UpdateSchemaVersionUp);
		}

		/// <summary>
		/// Migrates the database down to the targeted version.
		/// </summary>
		/// <param name="currentVersion">The current version of the database.</param>
		/// <param name="targetVersion">The targeted version of the database.</param>
		/// <param name="files">All migration script files.</param>
		private void MigrateDown(long currentVersion, long targetVersion, IEnumerable<IMigrationScriptFile> files)
		{
			IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts = files.OrderByDescending(x => x.Version)
				.Where(x => x.Version <= currentVersion && x.Version > targetVersion)
				.Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Teardown));

			ExecuteMigrationScripts(scripts, UpdateSchemaVersionDown);
		}

		private void ExecuteMigrationScripts(IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts, Action<DbTransaction, long> updateVersionAction)
		{
			using (DbTransaction tran = Database.BeginTransaction())
			{
				IMigrationScriptFile currentScript = null;
				try
				{
					foreach (var script in scripts)
					{
						currentScript = script.Key;
						Database.ExecuteScript(tran, script.Value);
						updateVersionAction(tran, script.Key.Version);
					}

					tran.Commit();
				}
				catch (Exception ex)
				{
					tran.Rollback();

					string filePath = (currentScript == null) ? "NULL" : currentScript.FilePath;
					throw new MigrationException("Error executing migration script: " + filePath, filePath, ex);
				}
			}
		}

		/// <summary>
		/// Updates the database with the version provided
		/// </summary>
		/// <param name="transaction">The transaction to execute the command in</param>
		/// <param name="version">The version to log</param>
		private static void UpdateSchemaVersionUp(DbTransaction transaction, long version)
		{
			const string sql = "INSERT INTO [schema_migrations] ([version]) VALUES ({0})";
			using (DbCommand cmd = transaction.CreateCommand())
			{
				cmd.CommandText = string.Format(sql, version);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Removes the provided version from the database log table.
		/// </summary>
		/// <param name="transaction">The transaction to execute the command in</param>
		/// <param name="version">The version to log</param>
		private static void UpdateSchemaVersionDown(DbTransaction transaction, long version)
		{
			const string sql = "DELETE FROM [schema_migrations] WHERE version = {0}";
			using (DbCommand cmd = transaction.CreateCommand())
			{
				cmd.CommandText = string.Format(sql, version);
				cmd.ExecuteNonQuery();
			}
		}
	}
}