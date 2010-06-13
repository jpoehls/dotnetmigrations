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

            TeardownDatabase();
        }

        #endregion

        private DataAccess _dataAccess;
        private DatabaseInitializer _subject;

        // TO TEST - Migration from legacy table

        [Test]
        public void Initialize_should_create_migration_table()
        {
            //  act
            _subject.Initialize();

            //  assert
            string results;
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                results = helper.ExecuteScalar("SELECT COUNT(Version) FROM [schema_migrations]")
                    .ToString();
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
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                results = helper.ExecuteScalar("SELECT COUNT(Version) FROM [schema_migrations]")
                    .ToString();
            }

            Assert.AreEqual("1", results);
        }
    }
}