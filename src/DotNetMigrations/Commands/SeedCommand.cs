using System;
using System.Data.Common;
using System.IO;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;

namespace DotNetMigrations.Commands
{
    public class SeedCommand : DatabaseCommandBase<SeedCommandArgs>
    {
        private readonly ISeedDirectory _seedDirectory;

        public override string CommandName
        {
            get { return "seed"; }
        }

        public override string Description
        {
            get { return "Plants seed data into the database."; }
        }

        public SeedCommand() : this(new SeedDirectory()) { }

        public SeedCommand(ISeedDirectory seedDirectory)
        {
            _seedDirectory = seedDirectory;
        }

        protected override void Execute(SeedCommandArgs args)
        {
            var seedScripts = _seedDirectory.GetScripts(args.Set);

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