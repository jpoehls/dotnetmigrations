using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using DotNetMigrations.Commands.Special;
using DotNetMigrations.Core;
using DotNetMigrations.Core.Data;

namespace DotNetMigrations.Commands
{
    internal class MigrateCommand : CommandBase
    {
        private const string DEFAULT_MIGRATION_SCRIPT_PATH = @".\migrate\";

        private DataAccess _da;

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
        /// Instantiates a new instance of teh MigrateCommand Class.
        /// </summary>
        public MigrateCommand()
        {
            _da = new DataAccess();
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            var migrationName = GetMigrationName();
            var connectionString = GetConnectionString(migrationName);
            var fileDictionary = CreateScriptFileDictionary(migrationName);
            var currentVersion = GetDatabaseVersion(migrationName, connectionString);
            var targetVersion = GetTargetScriptVersion(fileDictionary);

            if (currentVersion == -1 || targetVersion == -1 || currentVersion == targetVersion)
            {
                return;
            }

            if (currentVersion < targetVersion)
            {
                MigrateUp(currentVersion, targetVersion, fileDictionary, connectionString);
            }

            if (currentVersion > targetVersion)
            {
                MigrateDown(currentVersion, targetVersion, fileDictionary, connectionString);
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
            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number of arguments for the migrate command is too few.");
                return false;
            }

            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count > 4)
            {
                Log.WriteError("There are too many arguments for the migrate command.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the Migration name from the arguments.
        /// </summary>
        /// <returns>The migration name as string.</returns>
        private string GetMigrationName()
        {
            return Arguments.GetArgument(1);
        }

        /// <summary>
        /// Retrieves the connection string from the command arguments or the config file.
        /// </summary>
        /// <param name="migrationName">The migration name used to identify the string in the config file.</param>
        /// <returns>The connection string.</returns>
        private string GetConnectionString(string migrationName)
        {
            string connArg = null;

            if (Arguments.Count == 4)
            {
                connArg = Arguments.GetArgument(3);
            }

            return _da.GetConnectionString(migrationName, connArg);
        }

        /// <summary>
        /// Retrieves the current schema version of the database.
        /// </summary>
        /// <param name="migrationName">The migration name of the database to check.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The current version of the database.</returns>
        /// <remarks>If the [schema_migrations] database table doesn't exist, it will be created.</remarks>
        private long GetDatabaseVersion(string migrationName, string connectionString)
        {
            string command = "SELECT MAX([version]) FROM [schema_migrations]";

            long currentVersion = -1;
            string version = string.Empty;

            try
            {
                new CreateCommand(Log).Create(migrationName, connectionString);
                version = _da.ExecuteScalar<string>(connectionString, command).Trim();
                long.TryParse(version, out currentVersion);
            }
            catch (DbException ex)
            {
                Log.WriteError(ex.Message);
            }

            if (currentVersion != -1)
            {
                Log.WriteLine("Current Database Version:".PadRight(30) + version);
            }

            return currentVersion;
        }

        /// <summary>
        /// Returns the target version to migrate the database too. If the version is not provided
        /// from the command line arguments, the most recent script version will be used.
        /// </summary>
        /// <param name="fileDictionary">The directy of file names and versions</param>
        /// <returns>Returns the targeted version.</returns>
        private long GetTargetScriptVersion(Dictionary<long, string> fileDictionary)
        {
            long targetVersion;

            if (Arguments.Count >= 3 && long.TryParse(Arguments.GetArgument(2), out targetVersion))
            {
                return targetVersion;
            }

            return fileDictionary.Keys.First();
        }

        /// <summary>
        /// Migrates the database up to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        /// <param name="fileDictionary">The dictionary containing versions and file paths.</param>
        /// <param name="connectionString">The connection string to use.</param>
        private void MigrateUp(long currentVersion, long targetVersion, Dictionary<long, string> fileDictionary, string connectionString)
        {
            var files = (from f in fileDictionary
                         orderby f.Key
                         where f.Key > currentVersion && f.Key <= targetVersion
                         select new { Version = f.Key, Path = f.Value }).ToList();

            StringBuilder scriptLines;

            foreach (var file in files)
            {
                scriptLines = new StringBuilder();
                using (var reader = File.OpenText(file.Path))
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
                    Log.WriteWarning("SETUP was not found in migration version " + file.Version.ToString());
                }

                var sqlScripts = scriptLines.ToString().Split('|');
                foreach (var sql in sqlScripts)
                {
                    if (!string.IsNullOrEmpty(sql.Trim()))
                    {
                        _da.ExecuteNonQuery(connectionString, sql);
                    }
                }

                UpdateSchemaVersionUp(connectionString, file.Version);
                Log.WriteLine("Migrated to Version:".PadRight(30) + file.Version.ToString());
            }
        }

        /// <summary>
        /// Migrates the database down to the targeted version.
        /// </summary>
        /// <param name="currentVersion">The current version of the database.</param>
        /// <param name="targetVersion">The targeted version of the database.</param>
        /// <param name="fileDictionary">The dictionary containing versions and file paths.</param>
        /// <param name="connectionString">The connection string to use.</param>
        private void MigrateDown(long currentVersion, long targetVersion, Dictionary<long, string> fileDictionary, string connectionString)
        {
            var files = (from f in fileDictionary
                         orderby f.Key descending
                         where f.Key <= currentVersion && f.Key > targetVersion
                         select new { Version = f.Key, Path = f.Value }).ToList();

            StringBuilder scriptLines;

            foreach (var file in files)
            {
                scriptLines = new StringBuilder();
                using (var reader = File.OpenText(file.Path))
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
                    Log.WriteWarning("TEARDOWN was not found in migration version " + file.Version.ToString());
                }

                var sqlScripts = scriptLines.ToString().Split('|');
                foreach (var sql in sqlScripts)
                {
                    if (!string.IsNullOrEmpty(sql.Trim()))
                    {
                        _da.ExecuteNonQuery(connectionString, sql);
                    }
                }

                UpdateSchemaVersionDown(connectionString, file.Version);
                Log.WriteLine("Migrated From Version:".PadRight(30) + file.Version.ToString());
            }
        }

        /// <summary>
        /// Updates the database with the version provided
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="version">The version to log</param>
        private void UpdateSchemaVersionUp(string connectionString, long version)
        {
            var sql = string.Format("INSERT INTO [schema_migrations] ([version]) VALUES ({0})", version.ToString());
            _da.ExecuteNonQuery(connectionString, sql);
        }

        /// <summary>
        /// Removes the provided version from the database log table.
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="version">The version to log</param>
        private void UpdateSchemaVersionDown(string connectionString, long version)
        {
            var sql = string.Format("DELETE FROM [schema_migrations] WHERE version = {0}", version.ToString());
            _da.ExecuteNonQuery(connectionString, sql);
        }

        /// <summary>
        /// Reviews the migration directory and generates a Dictionary containing the version and the file path.
        /// </summary>
        /// <param name="migrationName">The migration name which is used to identify the scripts.</param>
        /// <returns>a Dictionary containing the version and the file path.</returns>
        private Dictionary<long, string> CreateScriptFileDictionary(string migrationName)
        {

            var scriptNamePattern = "*" + migrationName + ".sql";
            var migrationDirectory = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(migrationDirectory))
            {
                Log.WriteError("Migration directory not found.");
                return null;
            }

            var files = Directory.GetFiles(migrationDirectory, scriptNamePattern).ToList();

            if (files.Count == 0)
            {
                return null;
            }

            files.Sort();
            files.Reverse(); // place the list in descending order

            var dictionary = new Dictionary<long, string>();
            long key;

            foreach (var file in files)
            {
                key = GetVersionFromFileName(file);
                dictionary.Add(key, file);
            }

            return dictionary;
        }

        /// <summary>
        /// Retrieves the version number from the supplied file name.
        /// </summary>
        /// <param name="fileName">The file path and name of the file</param>
        /// <returns>The version number from the file name.</returns>
        private long GetVersionFromFileName(string fileName)
        {
            var pathParts = fileName.Split('\\');
            var version = pathParts[pathParts.Length - 1].Split('_')[0];

            long returnValue = -1;
            long.TryParse(version, out returnValue);

            return returnValue;
        }
    }
}
