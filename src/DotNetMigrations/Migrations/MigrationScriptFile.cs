using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetMigrations.Migrations
{
    public class MigrationScriptFile : IMigrationScriptFile
    {
        private const string SetupEndTag = "END_SETUP:";
        private const string SetupStartTag = "BEGIN_SETUP:";

        private const string TeardownEndTag = "END_TEARDOWN:";
        private const string TeardownStartTag = "BEGIN_TEARDOWN:";

        public MigrationScriptFile(string filePath)
        {
            FilePath = filePath;

            ParseVersion();
        }

        public string FilePath { get; private set; }
        public long Version { get; private set; }

        /// <summary>
        /// Parses and returns the contents of the migration script file.
        /// </summary>
        public MigrationScriptContents Read()
        {
            var setupBuilder = new StringBuilder();
            var teardownBuilder = new StringBuilder();

            string[] lines = File.ReadAllLines(FilePath);
            StringBuilder activeBuilder = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (string.Equals(line, SetupStartTag))
                    activeBuilder = setupBuilder;

                if (string.Equals(line, TeardownStartTag))
                    activeBuilder = teardownBuilder;

                if (string.Equals(line, SetupEndTag) ||
                    string.Equals(line, TeardownEndTag))
                    activeBuilder = null;

                if (activeBuilder != null)
                {
                    activeBuilder.AppendLine(line);
                }
            }

            var contents = new MigrationScriptContents(setupBuilder.ToString(),
                                                       teardownBuilder.ToString());
            return contents;
        }

        /// <summary>
        /// Writes the given contents into the migration script file.
        /// </summary>
        public void Write(MigrationScriptContents contents)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SetupStartTag);
            sb.AppendLine();
            sb.AppendLine(contents.Setup);
            sb.AppendLine();
            sb.AppendLine(SetupEndTag);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(TeardownStartTag);
            sb.AppendLine();
            sb.AppendLine(contents.Teardown);
            sb.AppendLine();
            sb.AppendLine(TeardownEndTag);

            File.WriteAllText(FilePath, sb.ToString());
        }

        private void ParseVersion()
        {
            string sVersion = Path.GetFileName(FilePath).Split('_').FirstOrDefault();
            long v;
            long.TryParse(sVersion, out v);
            Version = v;
        }

        public bool Equals(MigrationScriptFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Version == Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MigrationScriptFile)) return false;
            return Equals((MigrationScriptFile) obj);
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public static bool operator ==(MigrationScriptFile left, MigrationScriptFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MigrationScriptFile left, MigrationScriptFile right)
        {
            return !Equals(left, right);
        }
    }
}