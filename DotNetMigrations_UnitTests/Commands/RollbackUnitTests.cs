using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
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

            var arguments = new[] {"migrate", "testDb"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;
            migrateCommand.Arguments = args;

            CommandResults results = migrateCommand.Run();

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

        [Test]
        public void Should_Be_Able_To_Rollback_One_Version()
        {
            // migrate to a 2nd version
            MigrateUpToMigrateDown();

            var arguments = new[] {"rollback", "testDb"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;
            rollbackCommand.Arguments = args;

            CommandResults results = rollbackCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
        }

        [Test]
        public void Should_Not_Allow_Less_Than_2_Argments()
        {
            var arguments = new[] {"rollback"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;
            rollbackCommand.Arguments = args;

            CommandResults results = rollbackCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Not_Allow_More_Than_3_Argments()
        {
            var arguments = new[]
                                {"rollback", "migrationName", "connString", "something, something, something, darkside"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;
            rollbackCommand.Arguments = args;

            CommandResults results = rollbackCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Not_Be_Able_To_Rollback_Past_Initial_Schema()
        {
            var arguments = new[] {"rollback", "testDb"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var rollbackCommand = new RollbackCommand();
            rollbackCommand.Log = log;
            rollbackCommand.Arguments = args;

            CommandResults results = rollbackCommand.Run(); // roll back to 0

            args.Arguments.RemoveAt(2); // remove the argument that was added by the rollback command
            results = rollbackCommand.Run(); // attempt to rollback past 0

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(!DoesTestTableExist());
            Assert.AreEqual("0", _dbVersion);
        }
    }
}