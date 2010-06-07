using System;
using System.Linq;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class ArgumentSetUnitTests
    {
        [Test]
        public void NamedArgs_key_comparison_should_be_case_insensitive()
        {
            //  arrange
            var args = new[] {"-HELP", "migrate"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.IsTrue(set.NamedArgs.ContainsKey("help"));
        }

        [Test]
        public void Parse_should_assign_anonymous_arguments_by_position()
        {
        }

        [Test]
        public void Parse_should_find_anonymous_arguments_between_named_arguments()
        {
            //  arrange
            var args = new[] {"-help", "migrate", "joshua", "david", "-c", "connection"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count);
            Assert.AreEqual(2, set.AnonymousArgs.Count);
            Assert.AreEqual("joshua", set.AnonymousArgs[0]);
            Assert.AreEqual("david", set.AnonymousArgs[1]);
        }

        [Test]
        public void Parse_should_find_named_arguments_with_no_value()
        {
            //  arrange
            var args = new[] {"-help", "-me"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count);
            Assert.AreEqual(null, set.NamedArgs["help"]);
            Assert.AreEqual(null, set.NamedArgs["me"]);
        }

        [Test]
        public void Parse_should_match_dash_names()
        {
            //  arrange
            var args = new[] {"-help", "migrate"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.IsTrue(set.NamedArgs.ContainsKey("help"));
        }

        [Test]
        public void Parse_should_match_forward_slash_names()
        {
            //  arrange
            var args = new[] {"/help", "migrate"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.IsTrue(set.NamedArgs.ContainsKey("help"));
        }

        [Test]
        public void Parse_should_match_multiple_named_arguments_and_values()
        {
            //  arrange
            var args = new[] {"-help", "migrate", "-c", "connection"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count);
            Assert.AreEqual("migrate", set.NamedArgs["help"]);
            Assert.AreEqual("connection", set.NamedArgs["c"]);
        }
    }
}