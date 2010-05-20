using DotNetMigrations.Repositories;
using NUnit.Framework;
using Rhino.Mocks;

namespace DotNetMigrations.UnitTests.Repositories
{
    [TestFixture]
    public class ArgumentManagerUnitTests
    {
        [Test]
        public void Should_Get_Proper_Argument_Count()
        {
            var inputs = new string[] {"arg1", "arg2", "arg3"};
            var manager = new ArgumentRepository(inputs);

            Assert.AreEqual(inputs.Length, manager.Arguments.Count);
        }

        [Test]
        public void Should_Be_Able_To_Determine_If_An_Argument_Exists_By_Name()
        {
            var inputs = new string[] { "arg1", "arg2", "arg3" };
            var manager = new ArgumentRepository(inputs);

            Assert.IsTrue(manager.HasArgument("arg1"));
        }

        [Test]
        public void Should_Be_Able_To_Get_Argument_By_Index()
        {
            var inputs = new string[] { "arg1", "arg2", "arg3" };
            var manager = new ArgumentRepository(inputs);

            Assert.AreEqual("arg2", manager.GetArgument(1));
        }

        [Test]
        public void Should_Be_Able_To_Get_Last_Argument()
        {
            var inputs = new string[] { "arg1", "arg2", "arg3" };
            var manager = new ArgumentRepository(inputs);

            Assert.AreEqual("arg3", manager.GetLastArgument());
        }
    }
}
