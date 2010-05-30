using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.UnitTests
{
    public class DisposableDirectory : IDisposable
    {
        private readonly DirectoryInfo _dir;

        private DisposableDirectory(DirectoryInfo dir)
        {
            _dir = dir;
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

        /// <summary>
        /// Creates a directory at the given path and
        /// deletes it (and all contents) when disposed.
        /// </summary>
        public static DisposableDirectory Create(string path)
        {
            DirectoryInfo dir = Directory.CreateDirectory(path);
            return new DisposableDirectory(dir);
        }

        /// <summary>
        /// Watches the directory at the given path and
        /// deletes it (and all contents) when disposed.
        /// </summary>
        public static DisposableDirectory Watch(string path)
        {
            var dir = new DirectoryInfo(path);
            return new DisposableDirectory(dir);
        }
    }
}