using System;
using System.Linq;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class DatabaseCommandArgumentsUnitTests
    {
        [Test]
        public void Validation_should_fail_if_Connection_is_null()
        {
            //  arrange
            var argSet = ArgumentSet.Parse(new string[] { string.Empty });
            var args = new DatabaseCommandArguments();

            //  act
            args.Parse(argSet);

            //  assert
            Assert.IsFalse(args.IsValid);
            Assert.AreEqual(1, args.Errors.Count());
        }

        [Test]
        public void Validation_should_succeed_if_Connection_has_value()
        {
            //  arrange
            var argSet = ArgumentSet.Parse(new string[] { "-c", "my_connection" });
            var args = new DatabaseCommandArguments();

            //  act
            args.Parse(argSet);

            //  assert
            Assert.IsTrue(args.IsValid);
        }
    }
}