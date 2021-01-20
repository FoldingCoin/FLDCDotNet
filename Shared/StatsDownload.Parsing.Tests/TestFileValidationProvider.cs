﻿namespace StatsDownload.Parsing.Tests
{
    using System;

    using NSubstitute;

    using NUnit.Framework;

    using StatsDownload.Core.Interfaces;
    using StatsDownload.Core.Interfaces.DataTransfer;

    [TestFixture]
    public class TestFileValidationProvider
    {
        [SetUp]
        public void SetUp()
        {
            filePayloadMock = new FilePayload
                              {
                                  DownloadFilePath = "DownloadFilePath",
                                  DecompressedDownloadFilePath = "DecompressedDownloadFilePath"
                              };

            parseResultsMock = new ParseResults(DateTime.Today, null, null);

            fileCompressionServiceMock = Substitute.For<IFileCompressionService>();

            fileReaderServiceMock = Substitute.For<IFileReaderService>();

            statsFileParserServiceMock = Substitute.For<IStatsFileParserService>();
            statsFileParserServiceMock.Parse(filePayloadMock).Returns(parseResultsMock);

            systemUnderTest = new FileValidationProvider(fileCompressionServiceMock, fileReaderServiceMock,
                statsFileParserServiceMock);
        }

        private IFileCompressionService fileCompressionServiceMock;

        private FilePayload filePayloadMock;

        private IFileReaderService fileReaderServiceMock;

        private ParseResults parseResultsMock;

        private IStatsFileParserService statsFileParserServiceMock;

        private IFileValidationService systemUnderTest;

        [Test]
        public void ValidateFile_WhenInvoked_ReturnsValidationResults()
        {
            ParseResults actual = systemUnderTest.ValidateFile(filePayloadMock);

            Assert.That(actual, Is.EqualTo(parseResultsMock));
        }

        [Test]
        public void ValidateFile_WhenInvoked_SetsFileUtcDateTime()
        {
            systemUnderTest.ValidateFile(filePayloadMock);

            Assert.That(filePayloadMock.FileUtcDateTime, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public void ValidateFile_WhenInvoked_ValidatesFile()
        {
            systemUnderTest.ValidateFile(filePayloadMock);

            Received.InOrder(() =>
            {
                fileCompressionServiceMock.Received(1)
                                          .DecompressFile("DownloadFilePath", "DecompressedDownloadFilePath");
                fileReaderServiceMock.Received(1).ReadFile(filePayloadMock);
                statsFileParserServiceMock.Parse(filePayloadMock);
            });
        }
    }
}