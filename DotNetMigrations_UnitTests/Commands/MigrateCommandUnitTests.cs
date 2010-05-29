using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrateCommandUnitTests
    {
        private string _migrationPath;
        private string _firstScriptName;
        private string _secondScriptName;
        private string _dbVersion;
        private long _testTableVersion;

        #region Fixture and Test Setup

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            SetupInitialTestScript();
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        [SetUp]
        public void TestSetup()
        {
            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                string results = string.Empty;
                cmd.CommandText = "DROP TABLE [schema_migrations]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // do nothing
                }

                cmd.CommandText = "DROP TABLE [TestTable]";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        #endregion

        #region Argument Tests

        [Test]
        public void Should_Not_Allow_Less_Than_2_Argments()
        {
            var arguments = new string[] { "migrate" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migratCommand = new MigrateCommand();
            migratCommand.Log = log;
            migratCommand.Arguments = args;

            var results = migratCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Not_Allow_More_Than_4_Argments()
        {
            var arguments = new string[] { "migrate", "migrationName", "0", "connString", "something, something, something, darkside" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migratCommand = new MigrateCommand();
            migratCommand.Log = log;
            migratCommand.Arguments = args;

            var results = migratCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }

        #endregion

        #region Migrate Up Tests

        [Test]
        public void Should_Be_Able_To_Migrate_Up_One_Version()
        {
            var arguments = new string[] { "migrate", "testDb" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Up_To_Most_Recent()
        {
            SetupSecondaryTestScript();

            var arguments = new string[] { "migrate", "testDb" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_secondScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(2, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Up_To_Specific_Version()
        {
            var targetVersion = _firstScriptName.Split('_')[0] + "";

            SetupSecondaryTestScript();

            var arguments = new string[] { "migrate", "testDb", targetVersion };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        #endregion

        #region Migrate Down Tests

        [Test]
        public void Should_Be_Able_To_Migrate_Down_To_Specific_Version()
        {
            MigrateUpToMigrateDown();

            var targetVersion = _firstScriptName.Split('_')[0] + "";

            var arguments = new string[] { "migrate", "testDb", targetVersion};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Down_To_Original_Schema()
        {
            MigrateUpToMigrateDown();

            var targetVersion = 0.ToString();

            var arguments = new string[] { "migrate", "testDb", targetVersion };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(!DoesTestTableExist());
            Assert.AreEqual(targetVersion, _dbVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        #endregion

        private void SetupInitialTestScript()
        {
            _firstScriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_testDb.sql";
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(_migrationPath))
            {
                Directory.CreateDirectory(_migrationPath);
            }

            using (var writer = File.CreateText(Path.Combine(_migrationPath, _firstScriptName)))
            {
                writer.WriteLine("BEGIN_SETUP:\r\n\r\n\r\n");
                writer.WriteLine("CREATE TABLE [TestTable](Id INT NOT NULL)");
                writer.WriteLine("GO");
                writer.WriteLine("INSERT INTO [TestTable](Id) VALUES (1)");
                writer.WriteLine("GO");
                writer.WriteLine("END_SETUP:");
                writer.WriteLine("BEGIN_TEARDOWN:\r\n\r\n\r\n");
                writer.WriteLine("DROP TABLE [TestTable]");
                writer.WriteLine("GO");
                writer.WriteLine("END_TEARDOWN:");
            }
        }

        private void SetupSecondaryTestScript()
        {
            _secondScriptName = DateTime.Now.AddMinutes(2).ToString("yyyyMMddhhmmss") + "_testDb.sql";
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(_migrationPath))
            {
                Directory.CreateDirectory(_migrationPath);
            }

            using (var writer = File.CreateText(Path.Combine(_migrationPath, _secondScriptName)))
            {
                writer.WriteLine("BEGIN_SETUP:\r\n\r\n\r\n");
                writer.WriteLine("INSERT INTO [TestTable](Id) VALUES (2)");
                writer.WriteLine("GO");
                writer.WriteLine("END_SETUP:");
                writer.WriteLine("BEGIN_TEARDOWN:\r\n\r\n\r\n");
                writer.WriteLine("DELETE FROM [TestTable] WHERE Id = 2");
                writer.WriteLine("GO");
                writer.WriteLine("END_TEARDOWN:");
            }
        }

        private bool EnsureTableWasCreated()
        {
            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                string results = string.Empty;
                cmd.CommandText = "SELECT MAX(version) FROM [schema_migrations]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                try
                {
                    results = cmd.ExecuteScalar().ToString().Trim();
                }
                catch (SqlException)
                {
                    // do nothing
                }

                _dbVersion = results;

                return (!string.IsNullOrEmpty(results));
            }
        }

        private bool DoesTestTableExist()
        {
            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                string results = string.Empty;
                cmd.CommandText = "SELECT MAX(Id) FROM [TestTable]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                try
                {
                    results = cmd.ExecuteScalar().ToString();
                    _testTableVersion = long.Parse(results);
                }
                catch (SqlException)
                {
                    // do nothing
                }

                return (!string.IsNullOrEmpty(results));
            }
        }

        private void MigrateUpToMigrateDown()
        {
            // Create the 2nd script and migrate up
            SetupSecondaryTestScript();

            var arguments = new string[] { "migrate", "testDb" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            var results = migrateCommand.Run();

            if (results != CommandResults.Success)
            {
                Assert.Fail("Migration up failed.");
            }

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_secondScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(2, _testTableVersion);
        }
    }
}
