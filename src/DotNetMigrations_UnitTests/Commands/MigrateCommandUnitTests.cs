using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrateCommandUnitTests : DatabaseIntegrationTests
    {
        private MigrateCommandArgs _commandArgs;
        private MigrateCommand _migrateCommand;
        private MockLog1 _mockLog;
        private Mock<IMigrationDirectory> _mockMigrationDir;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _commandArgs = new MigrateCommandArgs();
            _commandArgs.Connection = TestConnectionString;

            //  create some test scripts

            _mockMigrationDir = new Mock<IMigrationDirectory>();
            //_mockMigrationDir.Setup(x => x.GetScripts()).Returns(migrationScriptPaths);
        }

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mockLog = new MockLog1();

            _migrateCommand = new MigrateCommand(_mockMigrationDir.Object);
            _migrateCommand.Log = _mockLog;
        }

        [TearDown]
        public void Teardown()
        {
            TeardownDatabase();
        }

        #endregion

        private string _migrationPath;
        private string _firstScriptName;
        private string _secondScriptName;
        private string _dbVersion;
        private long _testTableVersion;

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
                object results = helper.ExecuteScalar("SELECT MAX(Id) FROM [TestTable]");
                if (results != null)
                {
                    _testTableVersion = (long) results;
                }

                return results != null;
            }
        }

        private void MigrateUpToMigrateDown()
        {
            //  arrange
            //  create the 2nd script and migrate up
            SetupSecondaryTestScript();

            var log = new MockLog1();
            var args = new MigrateCommandArgs {Connection = "testDb"};

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_secondScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(2, _testTableVersion);
        }

        [Test]
        public void Run_should_migrate_to_latest_script_version_if_no_TargetVersion_is_given()
        {


            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_migrate_down_to_TargetVersion_if_less_than_current_schema_version()
        {
        throw new NotImplementedException();    
        }

        [Test]
        public void Run_should_migrate_up_to_TargetVersion_if_greater_than_current_schema_version()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Run_should_rollback_all_migrations_if_TargetVersion_is_0()
        {
            throw new NotImplementedException();
        }


        [Test]
        public void Should_Be_Able_To_Migrate_Down_To_Original_Schema()
        {
            //  arrange
            MigrateUpToMigrateDown();

            const long targetVersion = 0;
            var args = new MigrateCommandArgs
                           {
                               Connection = "testDb",
                               TargetVersion = targetVersion
                           };
            var log = new MockLog1();

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(!DoesTestTableExist());
            Assert.AreEqual(targetVersion, _dbVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Down_To_Specific_Version()
        {
            //  arrange
            MigrateUpToMigrateDown();

            long targetVersion = long.Parse(_firstScriptName.Split('_')[0] + "");

            var args = new MigrateCommandArgs
                           {
                               Connection = "testDb",
                               TargetVersion = targetVersion
                           };
            var log = new MockLog1();

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Up_One_Version()
        {
            //  arrange
            var log = new MockLog1();
            var args = new MigrateCommandArgs {Connection = "testDb"};

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Up_To_Most_Recent()
        {
            //  arrange
            SetupSecondaryTestScript();

            var args = new MigrateCommandArgs {Connection = "testDb"};
            var log = new MockLog1();

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_secondScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(2, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Be_Able_To_Migrate_Up_To_Specific_Version()
        {
            //  arrange
            long targetVersion = long.Parse(_firstScriptName.Split('_')[0] + "");

            SetupSecondaryTestScript();

            var args = new MigrateCommandArgs
                           {
                               Connection = "testDb",
                               TargetVersion = targetVersion
                           };
            var log = new MockLog1();

            var migrateCommand = new MigrateCommand();
            migrateCommand.Log = log;

            //  act
            migrateCommand.Run(args);

            //  assert
            Assert.IsTrue(EnsureTableWasCreated());
            Assert.IsTrue(DoesTestTableExist());
            Assert.AreEqual(_firstScriptName.Split('_')[0], _dbVersion);
            Assert.AreEqual(1, _testTableVersion);
            Assert.IsTrue(log.Output.Length > 1);
        }
    }
}