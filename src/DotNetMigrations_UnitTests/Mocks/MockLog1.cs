using System;
using System.Linq;
using System.Text;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    public class MockLog1 : LoggerBase
    {
        private readonly StringBuilder _logger;
        private string _logName = "MockLog";

        public MockLog1()
        {
            _logger = new StringBuilder();
        }

        public override string LogName
        {
            get { return _logName; }
            set { _logName = value; }
        }

        public string Output
        {
            get { return _logger.ToString(); }
        }

        public override void Write(string message)
        {
            _logger.Append(message);
        }

        public override void WriteLine(string message)
        {
            _logger.AppendLine(message);
        }

        public override void WriteWarning(string message)
        {
            _logger.AppendLine("WARNING: " + message);
        }

        public override void WriteError(string message)
        {
            _logger.AppendLine("ERROR: " + message);
        }

        public override void Dispose()
        {
            _logger.AppendLine("Disposed!");
        }
    }
}