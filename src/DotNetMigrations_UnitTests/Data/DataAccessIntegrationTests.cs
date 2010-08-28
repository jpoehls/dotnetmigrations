using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotNetMigrations.Core.Data;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Data
{
    [TestFixture]
    public class DataAccessIntegrationTests : DatabaseIntegrationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            CreateDatabase();
            _subject = DataAccessFactory.Create(TestConnectionString);
        }

        [TearDown]
        public void Teardown()
        {
            _subject.Dispose();

            TeardownDatabase();
        }

        #endregion

        private DataAccess _subject;

        [Test]
        public void BeginTransaction_should_return_DbTransaction_with_connection_set()
        {
            //  arrange
            _subject.OpenConnection();

            //  act
            DbTransaction tran = _subject.BeginTransaction();

            //  assert
            Assert.IsNotNull(tran.Connection);
        }

        [Test]
        public void CloseConnection_should_close_the_connection()
        {
            //  act
            _subject.OpenConnection();
            _subject.CloseConnection();
            DbCommand cmd = _subject.CreateCommand();

            //  assert
            Assert.IsTrue(cmd.Connection.State == ConnectionState.Closed);
        }

        [Test]
        public void CreateCommand_should_return_DbCommand_with_connection_set()
        {
            //  act
            DbCommand cmd = _subject.CreateCommand();

            //  assert
            Assert.IsNotNull(cmd.Connection);
        }

        [Test]
        public void DataAccess_connection_should_be_closed_when_instantiated()
        {
            //  arrange
            DbCommand cmd = _subject.CreateCommand();

            //  assert
            Assert.IsTrue(cmd.Connection.State == ConnectionState.Closed);
        }

        [Test]
        public void OpenConnection_should_open_the_connection()
        {
            //  act
            _subject.OpenConnection();
            DbCommand cmd = _subject.CreateCommand();

            //  assert
            Assert.IsTrue(cmd.Connection.State == ConnectionState.Open);
        }
    }
}