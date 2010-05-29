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
    public class GenerateScriptCommandUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Test_Setup()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        #endregion

        private string _migrationPath;

        [TestFixtureSetUp]
        public void Test_Fixture_Setup()
        {
            _migrationPath = ConfigurationManager.AppSettings["migrateFolder"];

            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        [TestFixtureTearDown]
        public void Test_Fixture_Teardown()
        {
            if (Directory.Exists(_migrationPath))
            {
                Directory.Delete(_migrationPath, true);
            }
        }

        [Test]
        public void Should_Create_Directory_And_Script()
        {
            const string migrationName = "testDb";
            var arguments = new[] {"Generate", migrationName};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var generateCommand = new GenerateScriptCommand();
            generateCommand.Log = log;
            generateCommand.Arguments = args;

            CommandResults results = generateCommand.Run();

            Assert.AreEqual(CommandResults.Success, results);
            Assert.IsTrue(Directory.Exists(_migrationPath));
            Assert.AreEqual(1, Directory.GetFiles(_migrationPath).Length);
            Assert.IsTrue(log.Output.Length > 1);
        }

        [Test]
        public void Should_Create_Directory_And_Script_Invalid_Arguments()
        {
            var arguments = new[] {"Generate"};
            var log = new MockLog1();
            var args = new ArgumentRepository(arguments);

            var generateCommand = new GenerateScriptCommand();
            generateCommand.Log = log;
            generateCommand.Arguments = args;

            CommandResults results = generateCommand.Run();

            Assert.AreEqual(CommandResults.Invalid, results);
            Assert.IsTrue(!Directory.Exists(_migrationPath));
            Assert.IsTrue(log.Output.Length > 1);
        }
    }
}