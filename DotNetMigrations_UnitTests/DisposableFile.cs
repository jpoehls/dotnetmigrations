using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.UnitTests
{
    public class DisposableFile : IDisposable
    {
        private readonly FileInfo _file;

        private DisposableFile(FileInfo file)
        {
            _file = file;
        }

        public string FullName
        {
            get { return _file.FullName; }
        }

        public string Name
        {
            get { return _file.Name; }
        }

        public string NameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(_file.Name); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_file.Exists)
            {
                _file.Delete();
            }
        }

        #endregion

        /// <summary>
        /// Watches the file at the given path and
        /// deletes it when disposed.
        /// </summary>
        public static DisposableFile Watch(string path)
        {
            var file = new FileInfo(path);
            return new DisposableFile(file);
        }
    }
}