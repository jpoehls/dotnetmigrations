using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class VersionCommandUnitTests
    {
        private string _migrationPath;
        private string _scriptName;
        private string _dbVersion;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            SetupTestScript();
            SetupDatabase();
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }

            var connString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand())
            {
                string results = string.Empty;
                cmd.CommandText = "DROP TABLE [schema_migrations]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public void Should_Report_DB_And_Script_Version_Invalid_Arguments()
        {
            var arguments = new string[] { "Version" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var versionCommand  = new VersionCommand();
            versionCommand.Log = log;
            versionCommand.Arguments = args;

            var results = versionCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Report_DB_And_Script_Version()
        {
            var expected = new StringBuilder();
            expected.AppendLine(string.Format("Current Database Version:".PadRight(30) + "{0}", _dbVersion));
            expected.AppendLine(string.Format("Current Script Version:".PadRight(30) + "{0}", _scriptName.Split('_')[0]));

            var arguments = new string[] { "Version", "testDb" };
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var versionCommand = new VersionCommand();
            versionCommand.Log = log;
            versionCommand.Arguments = args;

            var results = versionCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(log.Output.Length > 1);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.AreEqual(expected.ToString(), log.Output);
            
        }

        private void SetupTestScript()
        {
            _scriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_testDb.sql";
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(_migrationPath))
            {
                Directory.CreateDirectory(_migrationPath);
            }

            using (var writer = File.CreateText(Path.Combine(_migrationPath, _scriptName)))
            {
                writer.WriteLine("This is a test file.");
            }
        }

        private void SetupDatabase()
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
                    results = cmd.ExecuteScalar().ToString();    
                }
                catch(SqlException)
                {
                    // do nothing
                }

                if (string.IsNullOrEmpty(results))
                {
                    _dbVersion = "0";
                }
                else
                {
                    _dbVersion = results;
                }
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
                    results = cmd.ExecuteScalar().ToString();
                }
                catch (SqlException)
                {
                    // do nothing
                }

                return (!string.IsNullOrEmpty(results));
            }
        }
    }
}
