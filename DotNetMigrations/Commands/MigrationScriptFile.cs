using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.Commands
{
    public class MigrationScriptFile : IComparable<MigrationScriptFile>
    {
        public MigrationScriptFile(string filePath)
        {
            FilePath = filePath;

            ParseVersion();
        }

        private void ParseVersion()
        {
            var sVersion = Path.GetFileName(FilePath).Split('_').FirstOrDefault();
            long v;
            long.TryParse(sVersion, out v);
            Version = v;
        }

        public string FilePath { get; private set; }
        public long Version { get; private set; }

        public int CompareTo(MigrationScriptFile other)
        {
            return Version.CompareTo(other);
        }
    }
}