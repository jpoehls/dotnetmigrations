using System;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class CommandBaseUnitTests
    {
        [Test]
        public void CreateArguments_should_return_instance_of_generic_TArgs_type()
        {
            //  arrange
            var cmd = new MockCommand1();

            //  act
            IArguments args = cmd.CreateArguments();

            //  assert
            Type expectedType = typeof(MockCommandArgs);
            Assert.IsTrue(expectedType.IsInstanceOfType(args));
        }

        [Test]
        public void GetArgumentsType_should_return_generic_TArgs_type()
        {
            //  arrange
            var cmd = new MockCommand1();

            //  act
            Type argsType = cmd.GetArgumentsType();

            //  assert
            Type expectedType = typeof(MockCommandArgs);
            Assert.AreEqual(expectedType, argsType);
        }

        [Test]
        public void Run_should_throw_ArgumentException_if_args_type_doesnt_match_generic_TArgs()
        {
            //  arrange
            var cmd = new MockCommand1();
            var args = new DatabaseCommandArguments();

            //  act
            //  assert
            var ex = Assert.Throws<ArgumentException>(() => cmd.Run(args));
            Assert.AreEqual("args type doesn't match generic type\r\nParameter name: args", ex.Message);
        }

        [Test]
        public void Run_should_throw_ArgumentNullException_if_given_null_args()
        {
            //  arrange
            var cmd = new MockCommand1();

            //  act
            //  assert
            Assert.Throws<ArgumentNullException>(() => cmd.Run(null));
        }

        [Test]
        public void Run_should_throw_InvalidOperationException_if_args_are_not_valid()
        {
            //  arrange
            var cmd = new MockCommand1();
            cmd.Log = new MockLog1();
            ArgumentSet argSet = ArgumentSet.Parse(new[] { "blah" });
            var args = new MockCommandArgs();
            args.Parse(argSet);

            //  act
            var ex = Assert.Throws<InvalidOperationException>(() => cmd.Run(args));
            Assert.AreEqual("Argument validation failed. Arguments are invalid.", ex.Message);
        }

        [Test]
        public void Run_should_throw_InvalidOperationException_if_Log_property_is_null()
        {
            //  arrange
            var cmd = new MockCommand1();
            var args = new MockCommandArgs();

            //  act
            var ex = Assert.Throws<InvalidOperationException>(() => cmd.Run(args));
            Assert.AreEqual("ICommand.Log cannot be null.", ex.Message);
        }
    }
}