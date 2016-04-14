using System;
using System.Linq;
using DotNetMigrations.Commands;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Commands
{
    [TestFixture]
    public class MigrateCommandArgsUnitTests
    {
        [Test]
        public void Class_should_inherit_from_DatabaseCommandArguments()
        {
            //  assert
            Assert.IsTrue(typeof(DatabaseCommandArguments).IsAssignableFrom(typeof(MigrateCommandArgs)));
        }

        [Test]
        public void Constructor_should_default_TargetVersion_to_negative_1()
        {
            //  act
            var args = new MigrateCommandArgs();

            //  assert
            Assert.AreEqual(-1, args.TargetVersion);
        }

		[Test]
		public void Constructor_should_default_TransactionMode_to_PerRun()
		{
			//  act
			var args = new MigrateCommandArgs();

			//  assert
			Assert.AreEqual(MigrationTransactionMode.PerRun, args.TransactionMode);
		}
    }
}