using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.UnitTests
{
    /// <summary>
    /// Creates a directory at the given path and
    /// deletes it (and all contents) when disposed.
    /// </summary>
    public class DisposableDirectory : IDisposable
    {
        private readonly DirectoryInfo _dir;

        public DisposableDirectory(string path)
        {
            _dir = Directory.CreateDirectory(path);
        }

        public string FullName
        {
            get { return _dir.FullName; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_dir.Exists)
            {
                _dir.Delete(true);
            }
        }

        #endregion
    }
}