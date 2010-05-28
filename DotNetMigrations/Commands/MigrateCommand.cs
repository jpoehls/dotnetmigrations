using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class MigrateCommand : DatabaseCommandBase
    {
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
        public override string HelpText
        {
            get
            {
                return "Migrates the database up and down the versions."
                       + "\r\nExample: migrate <MigrateName> [Version] [ConnectionString]";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            base.RunCommand();

            IOrderedEnumerable<MigrationScriptFile> files = MigrationScriptHelper.GetScriptFiles()
                .OrderByDescending(x => x);

            if (files.Count() == 0)
            {
                return;
            }

            long currentVersion = GetDatabaseVersion();
            long targetVersion = GetTargetScriptVersion(files);

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
        /// Validates the arguments for the command
        /// </summary>
        /// <returns>True if the arguments are valid, else false.</returns>
        /// <remarks>
        /// Allowed Argument Structure:
        /// db.exe migrate migrationName [version] [connectionstring]
        /// </remarks>
        protected override bool ValidateArguments()
        {
            bool valid = base.ValidateArguments();
            if (!valid)
                return false;

            if (Arguments.Count > 4)
            {
                Log.WriteError("There are too many arguments for the migrate command.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the target version to migrate the database too. If the version is not provided
        /// from the command line arguments, the most recent script version will be used.
        /// </summary>
        /// <returns>Returns the targeted version.</returns>
        private long GetTargetScriptVersion(IEnumerable<MigrationScriptFile> scriptFiles)
        {
            long targetVersion;

            if (Arguments.Count >= 3 && long.TryParse(Arguments.GetArgument(2), out targetVersion))
            {
                return targetVersion;
            }

            return scriptFiles.Select(x => x.Version).First();
        }

        /// <summary>
        /// Migrates the database up to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        private void MigrateUp(long currentVersion, long targetVersion, IEnumerable<MigrationScriptFile> files)
        {
            IEnumerable<MigrationScriptFile> filesToUse = files.OrderBy(x => x)
                .Where(x => x.Version > currentVersion && x.Version <= targetVersion);

            StringBuilder scriptLines;

            foreach (MigrationScriptFile file in filesToUse)
            {
                scriptLines = new StringBuilder();
                using (StreamReader reader = File.OpenText(file.FilePath))
                {
                    string line = reader.ReadLine();
                    bool started = false;

                    while (line != null)
                    {
                        if (!started && line == "BEGIN_SETUP:")
                        {
                            started = true;
                        }
                        else if (started && line == "END_SETUP:")
                        {
                            break;
                        }
                        else if (started && line.Trim().ToUpper() == "GO")
                        {
                            scriptLines.Append("|");
                        }
                        else
                        {
                            scriptLines.AppendLine(line);
                        }

                        line = reader.ReadLine();
                    }
                }

                if (scriptLines.ToString().Trim().Length == 0)
                {
                    Log.WriteWarning("SETUP was not found in migration version " + file.Version);
                }

                string[] sqlScripts = scriptLines.ToString().Split('|');
                using (DbTransaction tran = DataAccess.BeginTransaction())
                {
                    try
                    {
                        foreach (string sql in sqlScripts)
                        {
                            if (!string.IsNullOrEmpty(sql.Trim()))
                            {
                                using (DbCommand cmd = tran.CreateCommand())
                                {
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        UpdateSchemaVersionUp(tran, file.Version);

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }

                Log.WriteLine("Migrated to Version:".PadRight(30) + file.Version);
            }
        }

        /// <summary>
        /// Migrates the database down to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        private void MigrateDown(long currentVersion, long targetVersion, IEnumerable<MigrationScriptFile> files)
        {
            IEnumerable<MigrationScriptFile> filesToUse = files.OrderBy(x => x)
                .Where(x => x.Version <= currentVersion && x.Version > targetVersion);

            StringBuilder scriptLines;

            foreach (MigrationScriptFile file in filesToUse)
            {
                scriptLines = new StringBuilder();
                using (StreamReader reader = File.OpenText(file.FilePath))
                {
                    string line = reader.ReadLine();
                    bool started = false;

                    while (line != null)
                    {
                        if (!started && line == "BEGIN_TEARDOWN:")
                        {
                            started = true;
                        }
                        else if (started && line == "END_TEARDOWN:")
                        {
                            break;
                        }
                        else if (started && line.Trim().ToUpper() == "GO")
                        {
                            scriptLines.Append("|");
                        }
                        else if (started)
                        {
                            scriptLines.AppendLine(line);
                        }

                        line = reader.ReadLine();
                    }
                }

                if (scriptLines.ToString().Trim().Length == 0)
                {
                    Log.WriteWarning("TEARDOWN was not found in migration version " + file.Version);
                }

                using (DbTransaction tran = DataAccess.BeginTransaction())
                {
                    try
                    {
                        string[] sqlScripts = scriptLines.ToString().Split('|');
                        foreach (string sql in sqlScripts)
                        {
                            if (!string.IsNullOrEmpty(sql.Trim()))
                            {
                                using (DbCommand cmd = tran.CreateCommand())
                                {
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        UpdateSchemaVersionDown(tran, file.Version);

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }

                Log.WriteLine("Migrated From Version:".PadRight(30) + file.Version);
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