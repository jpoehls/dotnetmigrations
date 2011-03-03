using System;
using System.Linq;
using DotNetMigrations.Core;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests
{
    [TestFixture]
    public class StringExtensionsUnitTests
    {
        [Test]
        public void Replace_should_work_for_single_occurance_and_redundant_replacement()
        {
            // arrange
            var str = "This is a SenTenCE.";

            // act
            str = str.Replace("sentence", "bad sentence", StringComparison.OrdinalIgnoreCase);

            // assert
            Assert.AreEqual("This is a bad sentence.", str);
        }

        [Test]
        public void Replace_should_work_for_multiple_occurances()
        {
            // arrange
            var str = "This is a a string.";

            // act
            str = str.Replace("a", "b", StringComparison.OrdinalIgnoreCase);

            // assert
            Assert.AreEqual("This is b b string.", str);
        }
    }
}