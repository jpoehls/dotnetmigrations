using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetMigrations.Core;
using DotNetMigrations.Commands;
using Moq;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Mocks;
using System.IO;

namespace DotNetMigrations.UnitTests.Commands
{
	[TestFixture]
	public class CombineCommandIntegrationTests
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_commandArgs = new CombineCommandArgs();

			_mockLog = new MockLog1();

			_mockMigrationScripts = new List<IMigrationScriptFile>();

			_mockMigrationDir = new Mock<IMigrationDirectory>();
			_mockMigrationDir.Setup(x => x.GetScripts()).Returns(() => _mockMigrationScripts);

			_combineCommand = new CombineCommand(_mockMigrationDir.Object);
			_combineCommand.Log = _mockLog;

			//  setup the mock migration scripts
			var mockScript1 = new Mock<IMigrationScriptFile>();
			mockScript1.SetupGet(x => x.Version).Returns(1);
			mockScript1.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
															   @"CREATE TABLE [TestTable] (Id INT NOT NULL)
                                                                GO
                                                                INSERT INTO [TestTable] (Id) VALUES (1)",
															   @"DROP TABLE [TestTable]"));
			_mockMigrationScripts.Add(mockScript1.Object);

			var mockScript2 = new Mock<IMigrationScriptFile>();
			mockScript2.SetupGet(x => x.Version).Returns(2);
			mockScript2.Setup(x => x.Read()).Returns(() => new MigrationScriptContents(
															   "INSERT INTO [TestTable] (Id) VALUES (2)",
															   "DELETE FROM [TestTable] WHERE Id = 2"));
			_mockMigrationScripts.Add(mockScript2.Object);
		}

		[TearDown]
		public void Teardown()
		{
		}

		#endregion

		private CombineCommandArgs _commandArgs;
		private CombineCommand _combineCommand;
		private MockLog1 _mockLog;
		private Mock<IMigrationDirectory> _mockMigrationDir;
		private List<IMigrationScriptFile> _mockMigrationScripts;

		[Test]
		public void Run_should_not_throw_an_exception_if_no_start_or_end_migrations_are_specified()
		{
			using(var tempfile = DisposableFile.Watch(Path.GetTempFileName()))
			{
				//  arrange
				_commandArgs.StartMigration = 0;
				_commandArgs.EndMigration = Int64.MaxValue;
				_commandArgs.OutputFile = tempfile.FullName;

				//  act/assert
				Assert.DoesNotThrow(() =>
				{
					_combineCommand.Run(_commandArgs);
				});
			}
		}

		[Test]
		public void Run_should_throw_argument_exception_if_no_output_file_is_specified()
		{
			//  arrange
			_commandArgs.OutputFile = null;

			//  act/assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				_combineCommand.Run(_commandArgs);
			});
		}

		[Test]
		public void Run_should_create_temporary_containing_both_mock_scripts()
		{
			using(var tempfile = DisposableFile.Watch(Path.GetTempFileName()))
			{
				//  arrange
				_commandArgs.StartMigration = 0;
				_commandArgs.EndMigration = Int64.MaxValue;
				_commandArgs.OutputFile = tempfile.FullName;

				//  act
				_combineCommand.Run(_commandArgs);

				// assert
				string outputfilecontents = File.ReadAllText(tempfile.FullName);
				Assert.IsNotNullOrEmpty(outputfilecontents);
				Assert.IsTrue(_mockMigrationScripts.Select(msf => msf.Read()).All(msc => outputfilecontents.Contains(msc.Setup)));
			}
		}

		// TODO: Are the correct scripts combined?
		// TODO: Try and run the scripts against a database? (too low level?)
		// TODO: Mess with parameters some more
	}
}
