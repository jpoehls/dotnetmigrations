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
            Type expectedType = typeof (MockCommandArgs);
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
            Type expectedType = typeof (MockCommandArgs);
            Assert.AreEqual(expectedType, argsType);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ArgumentException),
            ExpectedMessage = "args type doesn't match generic type\r\nParameter name: args")]
        public void Run_should_throw_ArgumentException_if_args_type_doesnt_match_generic_TArgs()
        {
            //  arrange
            var cmd = new MockCommand1();
            var args = new DatabaseCommandArguments();

            //  act
            cmd.Run(args);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ArgumentNullException))]
        public void Run_should_throw_ArgumentNullException_if_given_null_args()
        {
            //  arrange
            var cmd = new MockCommand1();

            //  act
            cmd.Run(null);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (InvalidOperationException),
            ExpectedMessage = "Argument validation failed. Arguments are invalid.")]
        public void Run_should_throw_InvalidOperationException_if_args_are_not_valid()
        {
            //  arrange
            var cmd = new MockCommand1();
            cmd.Log = new MockLog1();
            ArgumentSet argSet = ArgumentSet.Parse(new[] {"blah"});
            var args = new MockCommandArgs();
            args.Parse(argSet);

            //  act
            cmd.Run(args);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (InvalidOperationException),
            ExpectedMessage = "ICommand.Log cannot be null.")]
        public void Run_should_throw_InvalidOperationException_if_Log_property_is_null()
        {
            //  arrange
            var cmd = new MockCommand1();
            var args = new MockCommandArgs();

            //  act
            cmd.Run(args);
        }
    }
}