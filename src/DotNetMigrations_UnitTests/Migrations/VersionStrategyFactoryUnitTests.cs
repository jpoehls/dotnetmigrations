using System;
using System.Linq;
using DotNetMigrations.Core;
using DotNetMigrations.Migrations;
using DotNetMigrations.UnitTests.Stubs;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Migrations
{
    [TestFixture]
    public class VersionStrategyFactoryUnitTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _configManager = new InMemoryConfigurationManager();
            _subject = new VersionStrategyFactory(_configManager);
        }

        #endregion

        private VersionStrategyFactory _subject;
        private InMemoryConfigurationManager _configManager;

        //  should throw exception with invalid config value
        //  should return utc_time impl
        //  should return local_time impl

        [Test]
        public void GetStrategy_should_return_correct_local_time_implementation()
        {
            //  arrange
            _configManager.AppSettings[AppSettingKeys.VersionStrategy] = VersionStrategyFactory.LocalTime;

            //  act
            IVersionStrategy strategy = _subject.GetStrategy();

            //  assert
            Assert.IsInstanceOfType(typeof (LocalTimestampVersion), strategy);
        }

        [Test]
        public void GetStrategy_should_return_correct_utc_time_implementation()
        {
            //  arrange
            _configManager.AppSettings[AppSettingKeys.VersionStrategy] = VersionStrategyFactory.UtcTime;

            //  act
            IVersionStrategy strategy = _subject.GetStrategy();

            //  assert
            Assert.IsInstanceOfType(typeof (UtcTimestampVersion), strategy);
        }

        [Test]
        public void GetStrategy_should_return_correct_seq_num_implementation()
        {
            //  arrange
            _configManager.AppSettings[AppSettingKeys.VersionStrategy] = VersionStrategyFactory.SequentialNumber;

            //  act
            IVersionStrategy strategy = _subject.GetStrategy();

            //  assert
            Assert.IsInstanceOfType(typeof(SequentialNumberVersion), strategy);
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ApplicationException))]
        public void GetStrategy_should_throw_ApplicationException_if_config_setting_is_invalidS()
        {
            //  arrange
            _configManager.AppSettings[AppSettingKeys.VersionStrategy] = "i am not a valid option";

            //  act
            _subject.GetStrategy();
        }

        [Test]
        [ExpectedException(ExceptionType = typeof (ApplicationException))]
        public void GetStrategy_should_throw_ApplicationException_if_config_setting_is_missing()
        {
            //  act
            _subject.GetStrategy();
        }
    }
}