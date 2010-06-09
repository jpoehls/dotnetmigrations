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
    }
}