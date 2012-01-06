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

        [Test]
        public void ExecuteScript_should_perform_case_insensitive_replace_DNM_PROVIDER_token_in_script_with_the_current_ADO_provider_name()
        {
            //  arrange
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.ExecuteNonQuery("CREATE TABLE [providers] ([name] [nvarchar](100))");

                // act
                _subject.OpenConnection();
                using (var tran = _subject.BeginTransaction())
                {
                    _subject.ExecuteScript(tran, "INSERT INTO [providers] ([name]) VALUES ('/*dNm:PrOvIdEr*/')");
                    tran.Commit();
                }
                
                //  assert
                var providerNameInserted = helper.ExecuteScalar<string>("SELECT [name] FROM [providers]");
                Assert.AreEqual("System.Data.SqlServerCe.4.0", providerNameInserted);
            }
        }

        [Test]
        public void ExecuteScript_should_use_CommandTimeout_specified_in_the_connection_string()
        {
            // arrange
            _subject = DataAccessFactory.Create(TestConnectionString + ";CommandTimeout=1");

            // act
            _subject.OpenConnection();
            using (var tran = _subject.BeginTransaction())
            {
                try
                {
                    _subject.ExecuteScript(tran, "WaitFor Delay '00:00:01'");
                }
                catch(Exception ex)
                {
                    // assert

                    // Sql Server Compact (which is being used for these tests) does not
                    // support CommandTimeout values other than 0. So we will assume
                    // that if it barfs it is because our non-zero CommandTimeout was successfully (attempted to be) set.
                    Assert.IsInstanceOf<ArgumentException>(ex);
                    Assert.AreEqual("SqlCeCommand.CommandTimeout does not support non-zero values.", ex.Message);
                    return;
                }
            }

            Assert.Fail("Expected command timeout exception to be thrown.");
        }
    }
}