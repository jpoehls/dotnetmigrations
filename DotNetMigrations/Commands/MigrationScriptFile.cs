using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.Commands
{
    public class MigrationScriptFile : IComparable<MigrationScriptFile>, IEquatable<MigrationScriptFile>
    {
        public MigrationScriptFile(string filePath)
        {
            FilePath = filePath;

            ParseVersion();
        }

        public string FilePath { get; private set; }
        public long Version { get; private set; }

        #region IComparable<MigrationScriptFile> Members

        public int CompareTo(MigrationScriptFile other)
        {
            return Version.CompareTo(other.Version);
        }

        #endregion

        #region IEquatable<MigrationScriptFile> Members

        public bool Equals(MigrationScriptFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FilePath, FilePath);
        }

        #endregion

        private void ParseVersion()
        {
            string sVersion = Path.GetFileName(FilePath).Split('_').FirstOrDefault();
            long v;
            long.TryParse(sVersion, out v);
            Version = v;
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
            return (FilePath != null ? FilePath.GetHashCode() : 0);
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