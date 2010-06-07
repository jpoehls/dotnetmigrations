using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DotNetMigrations.Core
{
    public class SchemaException : ApplicationException
    {
        public SchemaException()
        {
        }

        public SchemaException(string message) : base(message)
        {
        }

        public SchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SchemaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}