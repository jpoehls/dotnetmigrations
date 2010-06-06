using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    internal class MigrateCommand : DatabaseCommandBase<MigrateCommandArgs>
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
            get
            {
                return "Migrates the database up and down the versions."
                       + "\r\nUsage: db migrate <connection_string> [target_version]";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void Run(MigrateCommandArgs args)
        {
            IOrderedEnumerable<MigrationScriptFile> files = _migrationDirectory.GetScripts()
                .OrderByDescending(x => x);

            if (files.Count() == 0)
            {
                return;
            }

            long currentVersion = GetDatabaseVersion();
            long targetVersion = args.TargetVersion;

            //  if version not provided, assume latest migration script version
            if (targetVersion == -1)
            {
                targetVersion = files.Select(x => x.Version).First();
            }

            if (currentVersion == -1 || targetVersion == -1 || currentVersion == targetVersion)
            {
                return;
            }

            if (currentVersion < targetVersion)
            {
                MigrateUp(currentVersion, targetVersion, files);
            }

            if (currentVersion > targetVersion)
            {
                MigrateDown(currentVersion, targetVersion, files);
            }

            Log.WriteLine("Database is now on version:".PadRight(30) + targetVersion);
        }

        /// <summary>
        /// Migrates the database up to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        private void MigrateUp(long currentVersion, long targetVersion, IEnumerable<MigrationScriptFile> files)
        {
            IEnumerable<KeyValuePair<long, string>> scripts = files.OrderBy(x => x)
                .Where(x => x.Version > currentVersion && x.Version <= targetVersion)
                .Select(x => new KeyValuePair<long, string>(x.Version, x.Read().Setup));

            using (DbTransaction tran = Database.BeginTransaction())
            {
                try
                {
                    foreach (var script in scripts)
                    {
                        Database.ExecuteScript(tran, script.Value);
                        UpdateSchemaVersionUp(tran, script.Key);
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            Log.WriteLine("Migrated to Version:".PadRight(30) + targetVersion);
        }

        /// <summary>
        /// Migrates the database down to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        private void MigrateDown(long currentVersion, long targetVersion, IEnumerable<MigrationScriptFile> files)
        {
            IEnumerable<KeyValuePair<long, string>> scripts = files.OrderByDescending(x => x)
                .Where(x => x.Version <= currentVersion && x.Version > targetVersion)
                .Select(x => new KeyValuePair<long, string>(x.Version, x.Read().Teardown));

            using (DbTransaction tran = Database.BeginTransaction())
            {
                try
                {
                    foreach (var script in scripts)
                    {
                        Database.ExecuteScript(tran, script.Value);
                        UpdateSchemaVersionDown(tran, script.Key);
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            Log.WriteLine("Migrated From Version:".PadRight(30) + targetVersion);
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