using System;
using System.Linq;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class ArgumentAttributeUnitTests
    {
        [Test]
        public void Constructor_should_initialize_properties()
        {
            //  arrange
            const string name = "name";
            const string shortName = "short_name";
            const string desc = "description";

            //  act
            var attr = new ArgumentAttribute(name, shortName, desc);

            //  assert
            Assert.AreEqual(name, attr.Name);
            Assert.AreEqual(shortName, attr.ShortName);
            Assert.AreEqual(desc, attr.Description);
        }

        [Test]
        public void Constructor_should_default_Position_to_max_int_value()
        {
            //  act
            var attr = new ArgumentAttribute(null, null, null);

            //  assert
            Assert.AreEqual(int.MaxValue, attr.Position);
        }
    }
}