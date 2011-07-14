using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Migrations;
using DotNetMigrations.Repositories;

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

			Log.WriteLine("Transaction mode is: " + args.TransactionMode.ToString() + ".");
			Log.WriteLine("");

            Log.WriteLine("Database is at version:".PadRight(30) + currentVersion);
			Log.WriteLine("Target version:".PadRight(30) + targetVersion);
			Log.WriteLine("");

            MigrationDirection direction;
            if (currentVersion < targetVersion)
            {
                direction = MigrationDirection.Up;
                MigrateUp(currentVersion, targetVersion, files, args.TransactionMode);
                Log.WriteLine("Migrated up to version:".PadRight(30) + targetVersion);
            }
            else if (currentVersion > targetVersion)
            {
                direction = MigrationDirection.Down;
				MigrateDown(currentVersion, targetVersion, files, args.TransactionMode);
                Log.WriteLine("Migrated down to version:".PadRight(30) + targetVersion);
            }
            else
            {
				Log.WriteLine("Your database is up-to-date!");
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
		/// <param name="transmode">The manner in which the migrations should be wrapped in transaction(s).</param>
		private void MigrateUp(long currentVersion, long targetVersion, IEnumerable<IMigrationScriptFile> files, MigrationTransactionMode transmode)
        {
            IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts = files.OrderBy(x => x.Version)
                .Where(x => x.Version > currentVersion && x.Version <= targetVersion)
                .Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Setup));

            ExecuteMigrationScripts(scripts, UpdateSchemaVersionUp, transmode);
        }

        /// <summary>
        /// Migrates the database down to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        /// <param name="files">All migration script files.</param>
		/// <param name="transmode">The manner in which the migrations should be wrapped in transaction(s).</param>
		private void MigrateDown(long currentVersion, long targetVersion, IEnumerable<IMigrationScriptFile> files, MigrationTransactionMode transmode)
        {
            IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts = files.OrderByDescending(x => x.Version)
                .Where(x => x.Version <= currentVersion && x.Version > targetVersion)
                .Select(x => new KeyValuePair<IMigrationScriptFile, string>(x, x.Read().Teardown));

            ExecuteMigrationScripts(scripts, UpdateSchemaVersionDown, transmode);
        }

        private void ExecuteMigrationScripts(IEnumerable<KeyValuePair<IMigrationScriptFile, string>> scripts, Action<DbTransaction, long> updateVersionAction, MigrationTransactionMode transmode)
        {
			Log.WriteLine(string.Format("{0} migrations to run.", scripts.Count()));

            using (DbTransaction outertran = (transmode == MigrationTransactionMode.PerRun ? Database.BeginTransaction() : null))
            {
                IMigrationScriptFile currentScript = null;
                try
                {
                    foreach (var script in scripts)
                    {
						Log.WriteLine(script.Key.FilePath);

						using (DbTransaction innertran = (transmode == MigrationTransactionMode.PerMigration ? Database.BeginTransaction() : null))
						{
							try
							{
								currentScript = script.Key;
								Database.ExecuteScript(outertran ?? innertran ?? null, script.Value);
								updateVersionAction(outertran ?? innertran ?? null, script.Key.Version);

								if (innertran != null) innertran.Commit();
							}
							catch (Exception)
							{
								// Just capture the exception, rollback the inner transaction (if one exists) and rethrow
								if (innertran != null) innertran.Rollback();
								throw;
							}
						}
                    }

					if (outertran != null) outertran.Commit();
                }
                catch (Exception ex)
                {
					if (outertran != null) outertran.Rollback();

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