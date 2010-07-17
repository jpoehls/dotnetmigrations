using System;
using System.IO;
using System.Linq;
using DotNetMigrations.Migrations;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Migrations
{
    [TestFixture]
    public class MigrationScriptFileUnitTests
    {
        [Test]
        public void Constructor_should_parse_path_correctly()
        {
            //  arrange
            const string path = "C:\\test\\temp\\1234_my_migration_script.sql";

            //  act
            var script = new MigrationScriptFile(path);

            //  assert
            Assert.AreEqual(path, script.FilePath);
        }

        [Test]
        public void Constructor_should_parse_version_when_present()
        {
            //  arrange
            const string path = "C:\\test\\temp\\1234_my_migration_script.sql";

            //  act
            var script = new MigrationScriptFile(path);

            //  assert
            const long expectedVersion = 1234;
            Assert.AreEqual(expectedVersion, script.Version);
        }

        [Test]
        public void Constructor_should_set_version_to_default_when_not_found_in_path()
        {
            //  arrange
            const string pathWithoutVersion = "C:\\test\\temp\\my_migration_script.sql";

            //  act
            var script = new MigrationScriptFile(pathWithoutVersion);

            //  assert
            const long expectedVersion = 0;
            Assert.AreEqual(expectedVersion, script.Version);
        }

        [Test]
        public void Equality_operator_should_find_two_with_the_same_FilePath_equal()
        {
            //  arrange
            const string path1 = "C:\\test\\123_my_migration_script.sql";
            var script1 = new MigrationScriptFile(path1);

            const string path2 = "C:\\test\\123_my_migration_script.sql";
            var script2 = new MigrationScriptFile(path2);

            //  act
            bool result = (script1 == script2);

            //  assertS
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_should_find_two_with_the_same_FilePath_equal()
        {
            //  arrange
            const string path1 = "C:\\test\\123_my_migration_script.sql";
            var script1 = new MigrationScriptFile(path1);

            const string path2 = "C:\\test\\123_my_migration_script.sql";
            var script2 = new MigrationScriptFile(path2);

            //  act
            bool result = script1.Equals(script2);

            //  assertS
            Assert.IsTrue(result);
        }

        [Test]
        public void Read_should_parse_file_contents_correctly()
        {
            //  arrange
            var tempFilePath = Path.GetTempFileName();
            using (DisposableFile.Watch(tempFilePath))
            {
                var file = new MigrationScriptFile(tempFilePath);
                const string setupText = "my setup text";
                const string teardownText = "my teardown text";
                file.Write(new MigrationScriptContents(setupText, teardownText));

                //  act
                var contents = file.Read();

                //  assert
                Assert.AreEqual(setupText, contents.Setup.Trim(), "setup does not match");
                Assert.AreEqual(teardownText, contents.Teardown.Trim(), "teardown does not match");
            }
        }
    }
}