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
        [ExpectedException(ExceptionType = typeof (InvalidOperationException),
            ExpectedMessage = "ICommand.Log cannot be null.")]
        public void Run_should_throw_InvalidOperationException_if_Log_property_is_null()
        {
            //  arrange
            var cmd = new MockCommand1();

            //  act
            cmd.Run();
        }
    }
}