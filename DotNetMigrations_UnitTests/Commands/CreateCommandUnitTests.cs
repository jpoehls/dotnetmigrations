using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DotNetMigrations.Commands.Special;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class CreateCommandUnitTests
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
        public void Should_Be_Able_To_Create_Migration_Table()
        {
            var migrationName = "testDb";
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var log = new MockLog1();

            var createCommand = new CreateCommand(log);

            createCommand.Create(migrationName, connectionString);

            string results;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "SELECT COUNT(Version) FROM [schema_migrations]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                results = cmd.ExecuteScalar().ToString();
            }

            Assert.AreEqual("1", results);
        }


        [Test]
        public void Should_Not_Create_Migration_Table_If_Exists()
        {
            var migrationName = "testDb";
            var connectionString = @"Data Source=.\SqlExpress;Initial Catalog=TestDb;Integrated Security=SSPI;";
            var log = new MockLog1();

            var createCommand = new CreateCommand(log);

            createCommand.Create(migrationName, connectionString);
            createCommand.Create(migrationName, connectionString);

            string results;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "SELECT COUNT(Version) FROM [schema_migrations]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                results = cmd.ExecuteScalar().ToString();
            }

            Assert.AreEqual("1", results);
        }

        // TO TEST - Migration from legacy table

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
}
