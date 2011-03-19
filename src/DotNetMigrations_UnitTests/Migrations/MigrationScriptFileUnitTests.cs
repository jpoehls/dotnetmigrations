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

        [Test]
        public void Read_should_parse_file_with_setup_tags_but_no_teardown_correctly()
        {
            //  arrange
            var tempFilePath = Path.GetTempFileName();
            using (DisposableFile.Watch(tempFilePath))
            {
                File.WriteAllText(tempFilePath, @"
BEGIN_SETUP:

    this is the setup

END_SETUP:

There is no teardown, this should be ignored.
");
                var file = new MigrationScriptFile(tempFilePath);

                //  act
                var contents = file.Read();

                //  assert
                Assert.IsNotNull(contents.Setup, "setup is null");
                Assert.AreEqual("this is the setup", contents.Setup.Trim(), "setup not parsed correctly");
                Assert.AreEqual(string.Empty, contents.Teardown, "teardown should be empty");
            }
        }

        [Test]
        public void Read_should_preserve_whitespace_in_scripts()
        {
            //  arrange
            var tempFilePath = Path.GetTempFileName();
            using (DisposableFile.Watch(tempFilePath))
            {
                File.WriteAllText(tempFilePath, @"
BEGIN_SETUP:

    this is the setup

END_SETUP:

BEGIN_TEARDOWN:

 one
  two
   
   three
    four

END_TEARDOWN:
");
                var file = new MigrationScriptFile(tempFilePath);

                //  act
                var contents = file.Read();

                // we are normalizing \r\n to just \n because for some reason
                // some \r\n's are returned as just \n from the regex matching

                //  assert
                Assert.AreEqual(@"
    this is the setup

".Replace("\r\n", "\n"), contents.Setup.Replace("\r\n", "\n"), "setup not parsed correctly");
                Assert.AreEqual(@"
 one
  two
   
   three
    four

".Replace("\r\n", "\n"), contents.Teardown.Replace("\r\n", "\n"), "teardown should be empty");
            }
        }

        [Test]
        public void Read_should_assume_entire_script_is_the_setup_if_no_tags_are_present()
        {
            //  arrange
            var tempFilePath = Path.GetTempFileName();
            using (DisposableFile.Watch(tempFilePath))
            {
                File.WriteAllText(tempFilePath, "this is the setup");
                var file = new MigrationScriptFile(tempFilePath);

                //  act
                var contents = file.Read();

                //  assert
                Assert.AreEqual("this is the setup", contents.Setup, "setup not parsed correctly");
                Assert.AreEqual(string.Empty, contents.Teardown, "teardown should be empty");
            }
        }

        [Test]
        public void Read_should_support_varied_casing_in_tag_names()
        {
            //  arrange
            var tempFilePath = Path.GetTempFileName();
            using (DisposableFile.Watch(tempFilePath))
            {
                File.WriteAllText(tempFilePath, @"
BeGiN_sETup:
this is the setup
enD_SEtUp:

BEgin_teARDOWn:
this is the teardown
eNd_tEaRdOWn:
");
                var file = new MigrationScriptFile(tempFilePath);

                //  act
                var contents = file.Read();

                //  assert
                Assert.AreEqual("this is the setup", contents.Setup.Trim(), "setup not parsed correctly");
                Assert.AreEqual("this is the teardown", contents.Teardown.Trim(), "teardown should be empty");
            }
        }
    }
}