using System;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using DotConsole;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    [Command("seed")]
    [Description("Plants seed data into the database.")]
    public class SeedCommand : DatabaseCommandBase
    {
        private readonly ISeedDirectory _seedDirectory;

        [Parameter("set", Flag = 's', Position = 1)]
        [Description("Set of seed data to plant.")]
        public string SetName { get; set; }

        public SeedCommand() : this(new SeedDirectory()) { }

        public SeedCommand(ISeedDirectory seedDirectory)
        {
            _seedDirectory = seedDirectory;
        }

        public override void Execute()
        {
            var seedScripts = _seedDirectory.GetScripts(SetName);

            // run the scripts
            using (DbTransaction tran = Database.BeginTransaction())
            {
                string currentScript = null;
                try
                {
                    Log.WriteLine("Planting...");
                    foreach (var script in seedScripts)
                    {
                        currentScript = script;
                        Log.WriteLine("  {0}", Path.GetFileName(script));
                        Database.ExecuteScript(tran, File.ReadAllText(script));
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    string filePath = currentScript ?? "NULL";
                    throw new ApplicationException("Error executing seed script: " + filePath, ex);
                }
            }
        }
    }
}