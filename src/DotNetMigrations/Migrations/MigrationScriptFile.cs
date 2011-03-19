using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetMigrations.Migrations
{
    public class MigrationScriptFile : IMigrationScriptFile
    {
        private const string SetupStartTag = "BEGIN_SETUP:";
        private const string SetupEndTag = "END_SETUP:";
        private const string TeardownStartTag = "BEGIN_TEARDOWN:";
        private const string TeardownEndTag = "END_TEARDOWN:";

        private static readonly Regex SetupRegex;
        private static readonly Regex TeardownRegex;

        static MigrationScriptFile()
        {
            SetupRegex =
                new Regex(
                    @"^\s*" + Regex.Escape(SetupStartTag) + @"\s*$  (.*)  ^\s*" + Regex.Escape(SetupEndTag) + @"\s*$",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.Compiled);

            TeardownRegex =
                new Regex(
                    @"^\s*" + Regex.Escape(TeardownStartTag) + @"\s*$  (.*)  ^\s*" + Regex.Escape(TeardownEndTag) +
                    @"\s*$",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.Compiled);
        }

        public MigrationScriptFile(string filePath)
        {
            FilePath = filePath;

            ParseVersion();
        }

        #region IMigrationScriptFile Members

        public string FilePath { get; private set; }
        public long Version { get; private set; }

        /// <summary>
        /// Parses and returns the contents of the migration script file.
        /// </summary>
        public MigrationScriptContents Read()
        {
            string setupScript = string.Empty;
            string teardownScript = string.Empty;

            string allLines = File.ReadAllText(FilePath);

            Match setupMatch = SetupRegex.Match(allLines);
            if (setupMatch.Success)
            {
                setupScript = setupMatch.Groups[1].Value;
            }

            // don't include the setup portion of the script
            // when matching the teardown
            Match teardownMatch = TeardownRegex.Match(allLines, setupMatch.Length);
            if (teardownMatch.Success)
            {
                teardownScript = teardownMatch.Groups[1].Value;
            }

            if (!setupMatch.Success && !teardownMatch.Success)
            {
                // assume entire file is the setup and there is no teardown
                setupScript = allLines;
            }

            return new MigrationScriptContents(setupScript, teardownScript);
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
            sb.AppendLine(TeardownStartTag);
            sb.AppendLine();
            sb.AppendLine(contents.Teardown);
            sb.AppendLine();
            sb.Append(TeardownEndTag);

            File.WriteAllText(FilePath, sb.ToString());
        }

        #endregion

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