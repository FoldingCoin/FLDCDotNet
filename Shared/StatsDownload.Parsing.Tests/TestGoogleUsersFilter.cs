﻿namespace StatsDownload.Parsing.Tests
{
    using System;
    using System.Linq;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using NSubstitute;

    using NUnit.Framework;

    using StatsDownload.Core.Interfaces;
    using StatsDownload.Core.Interfaces.DataTransfer;
    using StatsDownload.Core.Interfaces.Settings;
    using StatsDownload.Parsing.Filters;

    [TestFixture]
    public class TestGoogleUsersFilter
    {
        [SetUp]
        public void SetUp()
        {
            loggerMock = Substitute.For<ILogger<GoogleUsersFilter>>();

            innerServiceMock = Substitute.For<IStatsFileParserService>();

            filterSettings = new FilterSettings();

            filterSettingsOptionsMock = Substitute.For<IOptions<FilterSettings>>();
            filterSettingsOptionsMock.Value.Returns(filterSettings);

            systemUnderTest = new GoogleUsersFilter(loggerMock, innerServiceMock, filterSettingsOptionsMock);

            downloadDateTime = DateTime.UtcNow;
        }

        private DateTime downloadDateTime;

        private readonly FilePayload FilePayload = new FilePayload { DecompressedDownloadFileData = "fileData" };

        private FilterSettings filterSettings;

        private IOptions<FilterSettings> filterSettingsOptionsMock;

        private IStatsFileParserService innerServiceMock;

        private ILogger<GoogleUsersFilter> loggerMock;

        private IStatsFileParserService systemUnderTest;

        [Test]
        public void Parse_WhenDisabled_DoesNotModifyResults()
        {
            filterSettings.EnableGoogleUsersFilter = false;

            var expected = new ParseResults(downloadDateTime, null, null);
            innerServiceMock.Parse(FilePayload).Returns(expected);

            ParseResults actual = systemUnderTest.Parse(FilePayload);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WhenInvoked_FiltersResults()
        {
            filterSettings.EnableGoogleUsersFilter = true;

            innerServiceMock.Parse(FilePayload).Returns(new ParseResults(downloadDateTime,
                new[]
                {
                    new UserData(),
                    new UserData(0, "user", 0, 0, 0),
                    new UserData(0, "GOOGLE", 0, 0, 0),
                    new UserData(0, "Google", 0, 0, 0),
                    new UserData(0, "google", 0, 0, 0),
                    new UserData(0, "google123456", 0, 0, 0)
                }, new[] { new FailedUserData() }));

            ParseResults actual = systemUnderTest.Parse(FilePayload);

            Assert.That(actual.UsersData.Count(), Is.EqualTo(2));
            Assert.That(
                actual.UsersData.Count(data =>
                    data.Name?.StartsWith("google", StringComparison.OrdinalIgnoreCase) ?? false), Is.EqualTo(0));
        }
    }
}