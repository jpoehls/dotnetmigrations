using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using DotNetMigrations.UnitTests.Stubs;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class SeedDirectoryUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _configManager = new InMemoryConfigurationManager();
            _subject = new SeedDirectory(_configManager);
        }

        #endregion

        private SeedDirectory _subject;
        private InMemoryConfigurationManager _configManager;

        [Test]
        public void GetPath_should_create_path_if_it_doesnt_exist()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Watch(path))
            {
                _configManager.AppSettings[AppSettingKeys.SeedFolder] = path;

                //  act
                _subject.GetPath(null);

                //  assert
                bool pathExists = Directory.Exists(path);
                Assert.IsTrue(pathExists);
            }
        }

        [Test]
        public void GetPath_should_log_warning_if_seedFolder_appSetting_isnt_found()
        {
            //  arrange
            var logger = new MockLog1();

            //  act
            string path = _subject.GetPath(logger);
            using (DisposableDirectory.Watch(path))
            {
                //  assert
                Assert.IsTrue(logger.Output.StartsWith("WARNING"));
            }
        }

        [Test]
        public void GetPath_should_return_default_path_if_seedFolder_appSetting_isnt_found()
        {
            //  act
            string path = _subject.GetPath(null);
            using (DisposableDirectory.Watch(path))
            {
                //  assert
                bool pathExists = Directory.Exists(path);
                Assert.IsTrue(pathExists);
            }
        }

        [Test]
        public void GetScripts_should_return_all_SQL_scripts_in_seed_folder()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Create(path))
            {
                _configManager.AppSettings[AppSettingKeys.SeedFolder] = path;

                FileHelper.Touch(Path.Combine(path, "1_script_one.sql"));
                FileHelper.Touch(Path.Combine(path, "2_script_two.sql"));

                //  act
                IEnumerable<string> files = _subject.GetScripts(null);

                //  assert
                const int expectedCount = 2;
                Assert.AreEqual(expectedCount, files.Count());
            }
        }

        [Test]
        public void GetScripts_should_return_an_empty_enumerable_if_seed_folder_is_empty()
        {
            //  arrange
            string pathToEmptyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory emptyDir = DisposableDirectory.Create(pathToEmptyDir))
            {
                _configManager.AppSettings[AppSettingKeys.SeedFolder] = emptyDir.FullName;

                //  act
                IEnumerable<string> files = _subject.GetScripts(null);

                //  assert
                const int expectedCount = 0;
                Assert.AreEqual(expectedCount, files.Count());
            }
        }

        [Test]
        public void GetScripts_should_return_SQL_scripts_in_seed_folder_sorted_by_name()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Create(path))
            {
                _configManager.AppSettings[AppSettingKeys.SeedFolder] = path;

                FileHelper.Touch(Path.Combine(path, "1_script_one.sql"));
                FileHelper.Touch(Path.Combine(path, "2_script_two.sql"));
                FileHelper.Touch(Path.Combine(path, "10_script_ten.sql"));

                //  act
                IEnumerable<string> files = _subject.GetScripts(null);

                //  assert
                Assert.AreEqual("1_script_one", Path.GetFileNameWithoutExtension(files.First()));
                Assert.AreEqual("10_script_ten", Path.GetFileNameWithoutExtension(files.Last()));
            }
        }

        [Test]
        public void GetScripts_when_given_set_name_should_return_SQL_scripts_in_seed_folder_first_then_set_script_sorted_by_name()
        {
            //  arrange
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (DisposableDirectory.Create(path))
            {
                _configManager.AppSettings[AppSettingKeys.SeedFolder] = path;

                // seed root scripts
                FileHelper.Touch(Path.Combine(path, "1_script_one.sql"));
                FileHelper.Touch(Path.Combine(path, "2_script_two.sql"));
                FileHelper.Touch(Path.Combine(path, "10_script_ten.sql"));

                // set scripts
                var setPath = Path.Combine(path, "myset");
                Directory.CreateDirectory(setPath);

                FileHelper.Touch(Path.Combine(setPath, "1_set_one.sql"));
                FileHelper.Touch(Path.Combine(setPath, "2_set_two.sql"));
                FileHelper.Touch(Path.Combine(setPath, "10_set_ten.sql"));

                //  act
                IEnumerable<string> files = _subject.GetScripts("myset");

                //  assert
                Assert.AreEqual("1_script_one", Path.GetFileNameWithoutExtension(files.First()));
                Assert.AreEqual("10_script_ten", Path.GetFileNameWithoutExtension(files.Skip(2).First()));
                Assert.AreEqual("1_set_one", Path.GetFileNameWithoutExtension(files.Skip(3).First()));
                Assert.AreEqual("10_set_ten", Path.GetFileNameWithoutExtension(files.Last()));
            }
        }
    }
}