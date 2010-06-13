using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class GenerateScriptCommandArgsUnitTests
    {
        [Test]
        public void Validation_should_fail_if_MigrationName_is_null_or_empty()
        {
            //  arrange
            var argSet = ArgumentSet.Parse(new string[] { string.Empty });
            var args = new GenerateScriptCommandArgs();

            //  act
            args.Parse(argSet);

            //  assert
            Assert.IsFalse(args.IsValid);
            Assert.AreEqual(1, args.Errors.Count());
        }

        [Test]
        public void Validation_should_succeed_if_MigrationName_has_value()
        {
            //  arrange
            var argSet = ArgumentSet.Parse(new string[] { "-n", "my_migration_name" });
            var args = new GenerateScriptCommandArgs();

            //  act
            args.Parse(argSet);

            //  assert
            Assert.IsTrue(args.IsValid);
        }
    }
}