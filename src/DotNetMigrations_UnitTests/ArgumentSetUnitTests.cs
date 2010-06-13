using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class ArgumentSetUnitTests
    {
        [Test]
        public void ContainsName_should_be_case_insensitive()
        {
            //  arrange
            var args = new[] {"-HELP", "migrate"};
            ArgumentSet set = ArgumentSet.Parse(args);

            //  act
            bool exists = set.ContainsName("help");

            //  assert
            Assert.IsTrue(exists);
        }

        [Test]
        public void ContainsName_should_return_true_if_name_exists()
        {
            //  arrange
            var args = new[] {"-help", "migrate"};
            ArgumentSet set = ArgumentSet.Parse(args);

            //  act
            bool exists = set.ContainsName("help");

            //  assert
            Assert.IsTrue(exists);
        }

        [Test]
        public void GetByName_should_return_value_of_argument_with_given_name()
        {
            //  arrange
            var args = new[] {"-help", "migrate"};
            ArgumentSet set = ArgumentSet.Parse(args);

            //  act
            string value = set.GetByName("help");

            //  assert
            const string expectedValue = "migrate";
            Assert.AreEqual(expectedValue, value);
        }

        [Test]
        [ExpectedException(ExceptionType=typeof(KeyNotFoundException))]
        public void GetByName_should_throw_KeyNotFoundException_if_name_doesnt_exist()
        {
            //  arrange
            var args = new[] {"blah"};
            ArgumentSet set = ArgumentSet.Parse(args);

            //  act
            set.GetByName("help");
        }

        [Test]
        public void Parse_should_find_anonymous_arguments_between_named_arguments()
        {
            //  arrange
            var args = new[] {"-help", "migrate", "joshua", "david", "-c", "connection"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count());
            Assert.AreEqual(2, set.AnonymousArgs.Count());
            Assert.AreEqual("joshua", set.AnonymousArgs.First());
            Assert.AreEqual("david", set.AnonymousArgs.Skip(1).First());
        }

        [Test]
        public void Parse_should_find_named_arguments_with_no_value()
        {
            //  arrange
            var args = new[] {"-help", "-me"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count());
            Assert.AreEqual(null, set.GetByName("help"));
            Assert.AreEqual(null, set.GetByName("me"));
        }

        [Test]
        public void Parse_should_match_dash_names()
        {
            //  arrange
            var args = new[] {"-help", "migrate"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.IsTrue(set.ContainsName("help"));
        }

        [Test]
        public void Parse_should_match_forward_slash_names()
        {
            //  arrange
            var args = new[] {"/help", "migrate"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.IsTrue(set.ContainsName("help"));
        }

        [Test]
        public void Parse_should_match_multiple_named_arguments_and_values()
        {
            //  arrange
            var args = new[] {"-help", "migrate", "-c", "connection"};

            //  act
            ArgumentSet set = ArgumentSet.Parse(args);

            //  assert
            Assert.AreEqual(2, set.NamedArgs.Count());
            Assert.AreEqual("migrate", set.GetByName("help"));
            Assert.AreEqual("connection", set.GetByName("c"));
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ArgumentNullException))]
        public void Parse_should_throw_ArgumentNullException_if_args_param_is_null()
        {
            //  act
            ArgumentSet.Parse(null);
        }
    }
}