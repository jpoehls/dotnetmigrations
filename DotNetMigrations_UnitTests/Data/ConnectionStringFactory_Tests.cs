using System;
using System.Configuration;
using System.Linq;
using DotNetMigrations.Core.Data;
using DotNetMigrations.UnitTests.Stubs;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Data
{
    [TestFixture]
    public class ConnectionStringFactory_Tests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _configManager = new InMemoryConfigurationManager();
            _subject = new ConnectionStringFactory(_configManager);
        }

        [TearDown]
        public void TearDown()
        {
            //  remove the test connection string (if it exists)
            _configManager.ConnectionStrings.Remove(TestConnectionStringName);
        }

        #endregion

        private ConnectionStringFactory _subject;
        private InMemoryConfigurationManager _configManager;
        private const string TestConnectionStringName = "{770DEA10-3A6D-4fbc-8203-A2041719FE08}";

        [Test]
        public void GetConnectionString_should_return_connectionString_for_given_name()
        {
            //  arrange
            const string expectedConnStr = "Server=XYZ";
            _configManager.ConnectionStrings.Add(new ConnectionStringSettings(TestConnectionStringName,
                                                                              expectedConnStr));

            //  act
            string actualConnStr = _subject.GetConnectionString(TestConnectionStringName);

            //  assert
            Assert.AreEqual(expectedConnStr, actualConnStr);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ArgumentException))]
        public void GetConnectionString_should_throw_ArgumentException_when_name_doesnt_exist()
        {
            //  arrange
            const string fakeConnStrName = "{7DF4669C-86DE-459e-B5B2-53F1512B5536}";

            //  act
            _subject.GetConnectionString(fakeConnStrName);
        }

        [Test]
        public void IsConnectionString_should_return_False_if_string_doesnt_resemble_a_connectionString()
        {
            //  arrange
            const string fakeConnStr = "I AM NOT A CONNECTION STRING, HA!";

            //  act
            bool result = _subject.IsConnectionString(fakeConnStr);

            //  assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsConnectionString_should_return_False_if_string_is_null()
        {
            //  arrange
            const string fakeConnStr = null;

            //  act
            bool result = _subject.IsConnectionString(fakeConnStr);

            //  assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsConnectionString_should_return_True_if_string_has_delimited_parts_like_a_connectionString()
        {
            //  arrange
            const string realConnStr = "Server=XYZ";

            //  act
            bool result = _subject.IsConnectionString(realConnStr);

            //  assert
            Assert.IsTrue(result);
        }
    }
}