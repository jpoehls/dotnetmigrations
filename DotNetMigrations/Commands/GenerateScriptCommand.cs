using System;
using System.Configuration;
using System.ComponentModel.Composition;
using System.IO;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class GenerateScriptCommand : CommandBase
    {
        private const string DEFAULT_MIGRATION_SCRIPT_PATH = @".\migrate\";

        /// <summary>
        /// The name of the command that is typed as a command line argument.
        /// </summary>
        public override string CommandName
        {
            get { return "generate"; }
        }

        /// <summary>
        /// The help text information for the command.
        /// </summary>
        public override string HelpText
        {
            get
            {
                return "Generates a new migration script in the migration directory."
                                 + "\r\nExample: generate <MigrateName>";
            }
        }

        /// <summary>
        /// Executes the Command's logic.
        /// </summary>
        protected override void RunCommand()
        {
            string scriptPath = GetScriptPath();

            VerifyAndCreatePath(scriptPath);

            GenerateScript(scriptPath);
        }

        /// <summary>
        /// Validates the arguments for the command
        /// </summary>
        /// <returns>True if the arguments are valid, else false.</returns>
        protected override bool ValidateArguments()
        {
            // The 1st argument is the command name and the 2nd is the migration script name.
            if (Arguments.Count < 2)
            {
                Log.WriteError("The number of arguments for the Generate command is too few.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to determine the directory to place the migration scripts.
        /// </summary>
        /// <returns>The file path for the migration scripts.</returns>
        private string GetScriptPath()
        {
            string path = GetScriptPathFromConfig();

            if (string.IsNullOrEmpty(path))
            {
                Log.WriteWarning("The migration folder path was not present in the configuration file and the default .\\migrate\\ folder will be used instead.");
                path = DEFAULT_MIGRATION_SCRIPT_PATH;
            }

            return path;
        }

        /// <summary>
        /// Checks the config file to ensure the migration path is there
        /// </summary>
        /// <returns>Null if the path is null or empty</returns>
        private string GetScriptPathFromConfig()
        {
            string path = ConfigurationManager.AppSettings["migrateFolder"];

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            // append the trailing backslash if needed
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }

            return path;
        }

        /// <summary>
        /// Verify the path exists and creates it if it's missing.
        /// </summary>
        /// <param name="path">The path to verify.</param>
        private void VerifyAndCreatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Creates the .sql file and sends the final message.
        /// </summary>
        /// <param name="path">The directory to create the new script.</param>
        private void GenerateScript(string path)
        {
            string migrationName = Arguments.GetArgument(1);
            string scriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + migrationName + ".sql";

            string file = Path.Combine(path, scriptName);

            using (StreamWriter writer = File.CreateText(file))
            {
                writer.WriteLine("BEGIN_SETUP:\r\n\r\n\r\n");
                writer.WriteLine("END_SETUP:");
                writer.WriteLine("BEGIN_TEARDOWN:\r\n\r\n\r\n");
                writer.WriteLine("END_TEARDOWN:");
            }

            Log.WriteLine("The new migration script, " + scriptName + ", was created successfully!");
        }
    }
}
