using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DotNetMigrations.Commands
{
    public class MigrationException : ApplicationException
    {
        public MigrationException()
        {
        }

        public MigrationException(string message, string filePath) : base(message)
        {
            FilePath = filePath;
        }

        public MigrationException(string message, string filePath, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }

        protected MigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string FilePath { get; private set; }
    }
}