using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class RollbackUnitTests : DatabaseIntegrationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void TestSetup()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.SwallowSqlExceptions = true;
                helper.ExecuteNonQuery("DROP TABLE [schema_migrations]");
                helper.ExecuteNonQuery("DROP TABLE [TestTable]");
            }
        }

        #endregion

        private string _migrationPath;
        private string _firstScriptName;
        private string _secondScriptName;
        private string _dbVersion;
        private long _testTableVersion;

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

        private void SetupInitialTestScript()
        {
            _firstScriptName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_testDb.sql";
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (!Directory.Exists(_migrationPath))
            {
                Directory.CreateDirectory(_migrationPath);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(_migrationPath, _firstScriptName)))
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

            using (StreamWriter writer = File.CreateText(Path.Combine(_migrationPath, _secondScriptName)))
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
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.SwallowSqlExceptions = true;
                string results = helper.ExecuteScalar("SELECT MAX(version) FROM [schema_migrations]")
                    .ToString().Trim();
                _dbVersion = results;

                return (!string.IsNullOrEmpty(results));
            }
        }

        private bool DoesTestTableExist()
        {
            using (var helper = new SqlDatabaseHelper(TestConnectionString))
            {
                helper.SwallowSqlExceptions = true;
                string results = helper.ExecuteScalar("SELECT MAX(Id) FROM [TestTable]")
                    .ToString();
                _testTableVersion = long.Parse(results);

                return (!string.IsNullOrEmpty(results));
            }
        }

        private void MigrateUpToMigrateDown()
        {
            // Create the 2nd script and migrate up
            SetupSecondaryTestScript();

            var args = new MigrateCommandArgs
                           {
                               Connection = "testDb"
                           };
            var log = new MockLog1();

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            migrateCommand.Run(args);

            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_secondScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(2, _testTableVersion);
        }

        [Test]
        public void Should_Be_Able_To_Rollback_One_Version()
        {
            //  arrange
            //  migrate to a 2nd version
            MigrateUpToMigrateDown();

            var log = new MockLog1();
            var args = new DatabaseCommandArguments
                           {
                               Connection = "testDb"
                           };

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;

            //  act
            rollbackCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
        }

        [Test]
        public void Should_Not_Be_Able_To_Rollback_Past_Initial_Schema()
        {
            //  arrange
            var args = new DatabaseCommandArguments
                           {
                               Connection = "testDb"
                           };
            var log = new MockLog1();

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;

            rollbackCommand.Run(args); // roll back to 0

            //  act
            rollbackCommand.Run(args); //  attempt to rollback past 0

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(!DoesTestTableExist());
            Assert.AreEqual("0", _dbVersion);
        }
    }
}