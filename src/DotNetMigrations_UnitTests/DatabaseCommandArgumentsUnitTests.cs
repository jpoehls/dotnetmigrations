using System;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class DatabaseCommandArgumentsUnitTests
    {
        [Test]
        public void Validation_should_fail_if_Connection_is_null_or_empty()
        {
            //  arrange
            var validator = new DotConsole.DataAnnotationValidator();
            var cmd = new MockDatabaseCommand1();

            //  act
            bool valid = validator.ValidateParameters(cmd);

            //  assert
            Assert.IsFalse(valid);
            Assert.AreEqual(1, validator.ErrorMessages.Count());
        }

        [Test]
        public void Validation_should_succeed_if_Connection_has_value()
        {
            //  arrange
            var validator = new DotConsole.DataAnnotationValidator();
            var cmd = new MockDatabaseCommand1();
            cmd.Connection = "something";

            //  act
            bool valid = validator.ValidateParameters(cmd);

            //  assert
            Assert.IsTrue(valid);
        }
    }
}