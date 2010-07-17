using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DotNetMigrations.Core.Data
{
    public class SqlParseException : ApplicationException
    {
        public SqlParseException()
        {
        }

        public SqlParseException(string message) : base(message)
        {
        }

        public SqlParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}