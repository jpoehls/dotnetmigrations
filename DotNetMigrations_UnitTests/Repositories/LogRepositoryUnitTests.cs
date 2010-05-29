using DotNetMigrations.Logs;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DotNetMigrations.UnitTests.Repositories
{
    [TestFixture]
    public class LogRepositoryUnitTests
    {
        [Test]
        public void Should_Discover_And_Load_Local_Parts()
        {
            var logRepository = new LogRepository();

            Assert.AreEqual(1, logRepository.Logs.Count);
        }

        [Test]
        public void Should_Be_Able_To_Retrieve_Logs_By_Name()
        {
            var logRepository = new LogRepository();
            var mock = logRepository.GetLog("MockLog");

            Assert.IsTrue(mock is MockLog1);
        }

        [Test]
        public void Should_Be_Able_To_WriteLines_Characters_To_The_Log()
        {
            var logRepository = new LogRepository();
            logRepository.WriteLine("Text1");
            logRepository.WriteLine("Text2");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("Text1\r\nText2\r\n", mock.Output);
        }

        [Test]
        public void Should_Be_Able_To_WriteWarning_Characters_To_The_Log()
        {
            var logRepository = new LogRepository();
            logRepository.WriteWarning("Text1");
            logRepository.WriteWarning("Text2");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("WARNING: Text1\r\nWARNING: Text2\r\n", mock.Output);
        }

        [Test]
        public void Should_Be_Able_To_WriteError_Characters_To_The_Log()
        {
            var logRepository = new LogRepository();
            logRepository.WriteError("Text1");
            logRepository.WriteError("Text2");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("ERROR: Text1\r\nERROR: Text2\r\n", mock.Output);
        }

        [Test]
        public void Should_Be_Able_Disposable()
        {
            var logRepository = new LogRepository();
            
            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            logRepository.Dispose();

            Assert.AreEqual("Disposed!\r\n", mock.Output);
        }

        [Test]
        public void Should_Be_Able_To_Add_Logs_Manually()
        {
            var logRepository = new LogRepository();
            var mockLog2 = new MockLog1() { LogName = "MockLog2" };
            logRepository.Logs.Add(mockLog2);

            Assert.AreEqual(2, logRepository.Logs.Count);
        }

        [Test]
        public void Should_Be_Able_To_WriteLines_Characters_To_All_Logs()
        {
            var logRepository = new LogRepository();
            var mockLog2 = new MockLog1() { LogName = "MockLog2" };
            logRepository.Logs.Add(mockLog2);

            logRepository.WriteLine("Text1");
            logRepository.WriteLine("Text2");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("Text1\r\nText2\r\n", mock.Output);
            Assert.AreEqual("Text1\r\nText2\r\n", mockLog2.Output);
        }

        [Test]
        public void Should_Be_Able_To_WriteWarning_Characters_To_All_Logs()
        {
            var logRepository = new LogRepository();
            var mockLog2 = new MockLog1() { LogName = "MockLog2" };
            logRepository.Logs.Add(mockLog2);

            logRepository.WriteWarning("Text1");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("WARNING: Text1\r\n", mock.Output);
            Assert.AreEqual("WARNING: Text1\r\n", mockLog2.Output);
        }

        [Test]
        public void Should_Be_Able_To_WriteError_Characters_To_All_Logs()
        {
            var logRepository = new LogRepository();
            var mockLog2 = new MockLog1() { LogName = "MockLog2" };
            logRepository.Logs.Add(mockLog2);

            logRepository.WriteError("Text1");

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            Assert.AreEqual("ERROR: Text1\r\n", mock.Output);
            Assert.AreEqual("ERROR: Text1\r\n", mockLog2.Output);
        }

        [Test]
        public void Should_Dispose_Of_All_Logs()
        {
            var logRepository = new LogRepository();
            var mockLog2 = new MockLog1() { LogName = "MockLog2" };
            logRepository.Logs.Add(mockLog2);

            // Get a reference to our mock log
            var mock = logRepository.GetLog("MockLog") as MockLog1;

            logRepository.Dispose();

            Assert.AreEqual("Disposed!\r\n", mock.Output);
            Assert.AreEqual("Disposed!\r\n", mockLog2.Output);
        }
    }
}
