﻿using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using StatsDownload.Core.Implementations.Tested;
using StatsDownload.Core.Interfaces;
using StatsDownload.Core.Interfaces.DataTransfer;

namespace StatsDownload.Core.Tests
{
    [TestFixture]
    public class TestGoogleUsersFilter
    {
        [SetUp]
        public void SetUp()
        {
            innerServiceMock = Substitute.For<IStatsFileParserService>();

            settingsMock = Substitute.For<IGoogleUsersFilterSettings>();

            systemUnderTest = new GoogleUsersFilter(innerServiceMock, settingsMock);
        }

        private IStatsFileParserService innerServiceMock;

        private IStatsFileParserService systemUnderTest;

        private IGoogleUsersFilterSettings settingsMock;

        [Test]
        public void Parse_WhenDisabled_DoesNotModifyResults()
        {
            settingsMock.Enabled.Returns(false);

            var expected = new ParseResults(null, null);
            innerServiceMock.Parse("fileData").Returns(expected);

            var actual = systemUnderTest.Parse("fileData");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WhenInvoked_FiltersResults()
        {
            settingsMock.Enabled.Returns(true);

            innerServiceMock.Parse("fileData")
                            .Returns(
                                new ParseResults(
                                    new[]
                                    {
                                        new UserData(),
                                        new UserData("user", 0, 0, 0),
                                        new UserData("GOOGLE", 0, 0, 0),
                                        new UserData("Google", 0, 0, 0),
                                        new UserData("google", 0, 0, 0),
                                        new UserData("google123456", 0, 0, 0)
                                    }, new[] {new FailedUserData()}));

            ParseResults actual = systemUnderTest.Parse("fileData");

            Assert.That(actual.UsersData.Count(), Is.EqualTo(2));
            Assert.That(
                actual.UsersData.Count(
                    data => data.Name?.StartsWith("google", StringComparison.OrdinalIgnoreCase) ?? false),
                Is.EqualTo(0));
        }
    }
}