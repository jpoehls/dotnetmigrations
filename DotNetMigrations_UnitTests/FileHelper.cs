using System;
using System.IO;
using System.Linq;

namespace DotNetMigrations.UnitTests
{
    public static class FileHelper
    {
        /// <summary>
        /// Creates an empty file at the given path.
        /// </summary>
        public static void Touch(string path)
        {
            File.Create(path).Dispose();
        }
    }
}