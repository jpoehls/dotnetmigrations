﻿using System;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Repositories;
using DotNetMigrations.UnitTests.Mocks;
using NUnit.Framework;
using DotNetMigrations.UnitTests.Stubs;

namespace DotNetMigrations.UnitTests.Repositories
{
    [TestFixture]
    public class CommandManagerUnitTests
    {
        [Test]
        public void Should_Be_Able_To_Retrieve_A_Command_By_CommandName()
        {
			var configManager = new InMemoryConfigurationManager();
			var cmdRepository = new CommandRepository(configManager);
            ICommand results = cmdRepository.GetCommand("TestCommand");

            Assert.IsNotNull(results);
            Assert.IsTrue(results is MockCommand1);
        }

        [Test]
        public void Should_Be_Able_To_Retrieve_Null_For_An_Unknown_Command()
        {
			var configManager = new InMemoryConfigurationManager();
			var cmdRepository = new CommandRepository(configManager);
            ICommand results = cmdRepository.GetCommand("ThisIsNotACommand");

            Assert.IsNull(results);
        }

        [Test]
        public void Should_Discover_And_Load_Local_Parts()
        {
			var configManager = new InMemoryConfigurationManager();
			var cmdRepository = new CommandRepository(configManager);

            Assert.IsNotNull(cmdRepository.Commands);
            Assert.AreEqual(2, cmdRepository.Commands.Count);
        }
    }
}