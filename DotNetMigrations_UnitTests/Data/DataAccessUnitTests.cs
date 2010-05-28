using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DotNetMigrations.Core.Data;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Data
{
    /*
    [TestFixture]
    public class DataAccessUnitTests
    {
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

        [Test]
        public void Should_Be_Able_To_Get_Connection_String()
        {
            var migrationName = "testMigration";
            var connectionString = "ConnectionString";

            var da = new DataAccess();
            var results = da.GetConnectionString(migrationName, connectionString);

            Assert.AreEqual(connectionString, results);
        }

        [Test]
        public void Should_Be_Able_To_Get_Connection_String_By_MigrationName()
        {
            var migrationName = "testMigration";
            var expected = @"Provider=System.Data.Sql;Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";

            var da = new DataAccess();
            var results = da.GetConnectionString(migrationName);

            Assert.AreEqual(expected, results);
        }

        [Test]
        public void Should_Be_Able_To_Execute_Scalar()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT TestName FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar(connectionString, sql);

            Assert.AreEqual("DotNetMigrations", results.ToString());
        }

        [Test]
        public void Should_Be_Able_To_Execute_Scalar_WithCast()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT TestName FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar<string>(connectionString, sql);

            Assert.AreEqual("DotNetMigrations", results);
        }

        [Test]
        public void Should_Be_Able_To_Execute_Scalar_WithErrorsSuppressed()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT UnknownColumn FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar(connectionString, sql, true);

            Assert.AreEqual(null, results);
        }

        [Test]
        [ExpectedException(typeof(SqlException))]
        public void Should_Be_Able_To_Execute_Scalar_WithErrorsThrown()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT UnknownColumn FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar(connectionString, sql, false);
        }

        [Test]
        public void Should_Be_Able_To_Execute_Scalar_WithCast_WithErrorsSuppressed()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT UnknownColumn FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar<string>(connectionString, sql, true);

            Assert.AreEqual(null, results);
        }

        [Test]
        [ExpectedException(typeof(SqlException))]
        public void Should_Be_Able_To_Execute_Scalar_WithCast_WithErrorsThrown()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "SELECT UnknownColumn FROM TestData WHERE Id = 1";

            var da = new DataAccess();
            var results = da.ExecuteScalar<string>(connectionString, sql, false);

            Assert.AreEqual(null, results);
        }

        [Test]
        public void Should_Be_Able_To_Execute_NonQuery()
        {
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var sql = "INSERT INTO TestData (TestName) VALUES ('NonQueryTest')";
            string results;

            var da = new DataAccess();
            da.ExecuteNonQuery(connectionString, sql);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "SELECT TestName FROM TestData WHERE Id = 2";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                results = cmd.ExecuteScalar().ToString();
            }

            Assert.AreEqual("NonQueryTest", results);
        }

        private void SetupDatabase()
        {
            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "Create Table TestData (Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY, TestName VARCHAR(25) NOT NULL)";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO TestData (TestName) VALUES ('DotNetMigrations')";
                cmd.ExecuteNonQuery();
            }
        }

        private void TeardownDatabase()
        {
            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "Drop Table TestData";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
    */
}
