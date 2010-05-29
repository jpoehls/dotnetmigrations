using System;
using System.Linq;
using DotNetMigrations.Commands;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrationScriptFileUnitTests
    {
        [Test]
        public void CompareTo_should_find_two_with_the_same_version_equal()
        {
            //  arrange
            const string path1 = "C:\\test\\123_my_migration_script.sql";
            var script1 = new MigrationScriptFile(path1);

            const string path2 = "C:\\test\\123_another_migration_script.sql";
            var script2 = new MigrationScriptFile(path2);

            //  act
            int result = script1.CompareTo(script2);

            //  assert
            const int expectedResult = 0;
            Assert.AreEqual(expectedResult, result);
        }

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
    }
}