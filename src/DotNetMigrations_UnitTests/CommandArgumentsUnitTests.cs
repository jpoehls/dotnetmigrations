using System;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class CommandArgumentsUnitTests
    {
        [Test]
        public void Constructor_should_add_initial_error_message()
        {
            //  arrange
            var args = new MockCommandArgs();

            //  assert
            Assert.IsFalse(args.IsValid);
            const string expectedError = "Arguments have not been initialized.";
            Assert.AreEqual(expectedError, args.Errors.First());
        }

        [Test]
        public void Parse_should_override_anonymous_arguments_with_named_arguments()
        {
            //  arrange
            ArgumentSet args = ArgumentSet.Parse(new[] {"joshua", "2", "-connection", "david", "-v", "123"});
            var opts = new MockCommandArgs();

            //  act
            opts.Parse(args);

            //  assert
            Assert.AreEqual("david", opts.Connection);
            Assert.AreEqual(123, opts.TargetVersion);
        }

        [Test]
        public void Parse_should_set_property_values_by_position_of_anonymous_arguments()
        {
            //  arrange
            ArgumentSet args = ArgumentSet.Parse(new[] {"joshua", "2"});
            var opts = new MockCommandArgs();

            //  act
            opts.Parse(args);

            //  assert
            Assert.AreEqual("joshua", opts.Connection);
            Assert.AreEqual(2, opts.TargetVersion);
        }

        [Test]
        public void Parse_should_set_property_values_to_matching_named_arguments()
        {
            //  arrange
            ArgumentSet args = ArgumentSet.Parse(new[] {"-connection", "joshua", "-v", "123"});
            var opts = new MockCommandArgs();

            //  act
            opts.Parse(args);

            //  assert
            Assert.AreEqual("joshua", opts.Connection);
            Assert.AreEqual(123, opts.TargetVersion);
        }

        [Test]
        public void Parse_should_validate_properties()
        {
            //  arrange
            ArgumentSet args = ArgumentSet.Parse(new[] {"-v", "1"});
            var opts = new MockCommandArgs();

            //  act
            opts.Parse(args);

            //  assert
            Assert.IsFalse(opts.IsValid);
        }

        [Test]
        public void Parse_should_validate_arguments_and_add_error_messages_to_collection()
        {
            //  arrange
            ArgumentSet args = ArgumentSet.Parse(new[] {"-v", "0"});
            var opts = new MockCommandArgs();

            //  act
            opts.Parse(args);

            //  assert
            Assert.AreEqual(2, opts.Errors.Count());
            Assert.AreEqual("Connection is required", opts.Errors.First());
            Assert.AreEqual("Target version must be between 1 and 5", opts.Errors.Last());
        }

        [Test]
        public void GetArgumentProperties_should_return_all_properties_with_an_ArgumentAttribute()
        {
            //  act
            var props = CommandArguments.GetArgumentProperties(typeof(MockCommandArgs));

            //  assert
            Assert.AreEqual(2, props.Count);
        }

        [Test]
        public void GetArgumentProperties_should_return_argument_properties_ordered_by_their_Position_value()
        {
            //  act
            var props = CommandArguments.GetArgumentProperties(typeof (MockCommandArgs));

            //  assert
            Assert.AreEqual(1, props.First().Value.Position);
            Assert.AreEqual(2, props.Skip(1).First().Value.Position);
        }
    }
}