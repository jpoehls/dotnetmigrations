using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.UnitTests.Mocks;
using DotNetMigrations.UnitTests.Stubs;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrationScriptHelperUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _configManager = new InMemoryConfigurationManager();
            _subject = new MigrationScriptHelper(_configManager);
        }

        #endregion

        private MigrationScriptHelper _subject;
        private InMemoryConfigurationManager _configManager;

        [Test]
        public void GetScriptFiles_should_return_all_SQL_scripts_in_folder()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Create(path))
            {
                _configManager.AppSettings["migrateFolder"] = path;

                FileHelper.Touch(Path.Combine(path, "1_script_one.sql"));
                FileHelper.Touch(Path.Combine(path, "2_script_two.sql"));

                //  act
                IEnumerable<MigrationScriptFile> files = _subject.GetScriptFiles();

                //  assert
                const int expectedCount = 2;
                Assert.AreEqual(expectedCount, files.Count());
            }
        }

        [Test]
        public void GetScriptFiles_should_return_an_empty_enumerable_if_folder_is_empty()
        {
            //  arrange
            string pathToEmptyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory emptyDir = DisposableDirectory.Create(pathToEmptyDir))
            {
                _configManager.AppSettings["migrateFolder"] = emptyDir.FullName;

                //  act
                IEnumerable<MigrationScriptFile> files = _subject.GetScriptFiles();

                //  assert
                const int expectedCount = 0;
                Assert.AreEqual(expectedCount, files.Count());
            }
        }

        [Test]
        public void GetScriptFiles_should_return_SQL_scripts_sorted_by_version()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Create(path))
            {
                _configManager.AppSettings["migrateFolder"] = path;

                FileHelper.Touch(Path.Combine(path, "1_script_one.sql"));
                FileHelper.Touch(Path.Combine(path, "2_script_two.sql"));
                FileHelper.Touch(Path.Combine(path, "10_script_ten.sql"));

                //  act
                IEnumerable<MigrationScriptFile> files = _subject.GetScriptFiles();

                //  assert
                Assert.IsTrue(files.First().Version == 1);
                Assert.IsTrue(files.Last().Version == 10);
            }
        }

        [Test]
        public void GetScriptPath_should_create_path_if_it_doesnt_exist()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Watch(path))
            {
                _configManager.AppSettings["migrateFolder"] = path;

                //  act
                _subject.GetScriptPath(null);

                //  assert
                bool pathExists = Directory.Exists(path);
                Assert.IsTrue(pathExists);
            }
        }

        [Test]
        public void GetScriptPath_should_log_warning_if_migrateFolder_appSetting_isnt_found()
        {
            //  arrange
            var logger = new MockLog1();

            //  act
            string path = _subject.GetScriptPath(logger);
            using (DisposableDirectory.Watch(path))
            {
                //  assert
                Assert.IsTrue(logger.Output.StartsWith("WARNING"));
            }
        }

        [Test]
        public void GetScriptPath_should_return_default_path_if_migrateFolder_appSetting_isnt_found()
        {
            //  act
            string path = _subject.GetScriptPath(null);
            using (DisposableDirectory.Watch(path))
            {
                //  assert
                bool pathExists = Directory.Exists(path);
                Assert.IsTrue(pathExists);
            }
        }
    }
}