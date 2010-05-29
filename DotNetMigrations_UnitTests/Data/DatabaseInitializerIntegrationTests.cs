using System;
using System.Data.SqlClient;
using System.Linq;
using DotNetMigrations.Core.Data;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Data
{
    [TestFixture]
    public class DatabaseInitializerIntegrationTests : DatabaseIntegrationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _dataAccess = DataAccessFactory.Create(TestConnectionString);
            _dataAccess.OpenConnection();
            _subject = new DatabaseInitializer(_dataAccess);
        }

        [TearDown]
        public void Teardown()
        {
            _dataAccess.Dispose();
        }

        #endregion

        private DataAccess _dataAccess;
        private DatabaseInitializer _subject;

        [TestFixtureSetUp]
        public void Test_Fixture_Setup()
        {
            SetupDatabase();
        }

        [TestFixtureTearDown]
        public void Test_Fixture_Teardown()
        {
            TeardownDatabase();
        }

        // TO TEST - Migration from legacy table

        private void SetupDatabase()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.ExecuteNonQuery(
                    "Create Table TestData (Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY, TestName VARCHAR(25) NOT NULL)");
                helper.ExecuteNonQuery("INSERT INTO TestData (TestName) VALUES ('DotNetMigrations')");
            }
        }

        private void TeardownDatabase()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.ExecuteNonQuery("Drop Table TestData");
            }
        }

        [Test]
        public void Initialize_should_create_migration_table()
        {
            //  act
            _subject.Initialize();

            //  assert
            string results;
            using (var conn = new SqlConnection(TestConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "SELECT COUNT(Version) FROM [schema_migrations]";
                cmd.Connection = conn;

                conn.Open();
                results = cmd.ExecuteScalar().ToString();
            }

            Assert.AreEqual("1", results);
        }

        [Test]
        public void Initialize_should_not_create_migration_table_if_it_exists()
        {
            //  arrange
            _subject.Initialize();

            //  act
            _subject.Initialize();

            //  assert
            string results;
            using (var conn = new SqlConnection(TestConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "SELECT COUNT(Version) FROM [schema_migrations]";
                cmd.Connection = conn;

                conn.Open();
                results = cmd.ExecuteScalar().ToString();
            }

            Assert.AreEqual("1", results);
        }
    }
}