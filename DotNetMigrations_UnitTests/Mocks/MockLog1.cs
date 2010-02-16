using System.Text;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests
{
    public class MockLog1 : LoggerBase
    {
        private StringBuilder _logger;
        private string _logName = "MockLog";

        public override string LogName
        {
            get { return _logName; }
            set { _logName = value; }
        }

        public string Output
        {
            get { return _logger.ToString(); }
        }

        public MockLog1()
        {
            _logger = new StringBuilder();
        }

        public override void WriteLine(string message)
        {
            //no line break
            _logger.AppendLine(message);
        }

        public override void WriteWarning(string message)
        {
            // with linebreak
            _logger.AppendLine("WARNING: " + message);
        }

        public override void WriteError(string message)
        {
            // For the purpose of the test will be the same as writeline
            _logger.AppendLine("ERROR: " + message);
        }

        public override void Dispose()
        {
            _logger.AppendLine("Disposed!");
        }
    }
}
