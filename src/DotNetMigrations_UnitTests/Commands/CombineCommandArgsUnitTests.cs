using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetMigrations.Core;
using DotNetMigrations.Commands;

namespace DotNetMigrations.UnitTests.Commands
{
	[TestFixture]
	public class CombineCommandArgsUnitTests
	{
		[Test]
		public void Class_should_inherit_from_CommandArguments()
		{
			//  assert
			Assert.IsTrue(typeof(CommandArguments).IsAssignableFrom(typeof(CombineCommandArgs)));
		}

		[Test]
		public void Constructor_should_default_EndMigration_to_largest_possible_Int64()
		{
			//  act
			var args = new CombineCommandArgs();

			//  assert
			Assert.AreEqual(Int64.MaxValue, args.EndMigration);
		}
	}
}
