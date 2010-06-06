using System;
using System.IO;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class CommandHelpWriterUnitTests
    {
        [Test]
        public void WriteOptionList_should_include_all_options_with_their_ShortName_Name_and_Description()
        {
            //  arrange
            using (var writer = new StringWriter())
            {
                var helpWriter = new CommandHelpWriter();

                //  act
                helpWriter.WriteOptionList(typeof(MockCommandArgs), writer);

                //  assert
                string output = writer.ToString();
                Assert.AreEqual("Options:\r\n"
                                + "\t-c, -connection\t\tName of the connection to use\r\n"
                                + "\t-v, -version\t\tVersion to migrate up or down to\r\n",
                                output);
            }
        }

        [Test]
        public void WriteOptionSyntax_should_include_all_arguments_with_brackets_around_optional_arguments()
        {
            //  arrange
            using (var writer = new StringWriter())
            {
                var helpWriter = new CommandHelpWriter();

                //  act
                helpWriter.WriteOptionSyntax(typeof(MockCommandArgs), writer);

                //  assert
                string output = writer.ToString();
                Assert.AreEqual("-c connection_value [-v version]", output);
            }
        }

        [Test]
        public void WriteOptionSyntax_should_use_Name_as_the_value_name_when_ValueName_is_null()
        {
            //  arrange
            using (var writer = new StringWriter())
            {
                var helpWriter = new CommandHelpWriter();

                //  act
                helpWriter.WriteOptionSyntax(typeof(MockCommandArgs), writer);

                //  assert
                string output = writer.ToString();
                Assert.AreEqual("-c connection_value [-v version]", output);
            }
        }

        [Test]
        public void WriteOptionSyntax_should_use_ValueName_when_provided()
        {
            //  arrange
            using (var writer = new StringWriter())
            {
                var helpWriter = new CommandHelpWriter();

                //  act
                helpWriter.WriteOptionSyntax(typeof(MockCommandArgs), writer);

                //  assert
                string output = writer.ToString();
                Assert.AreEqual("-c connection_value [-v version]", output);
            }
        }
    }
}