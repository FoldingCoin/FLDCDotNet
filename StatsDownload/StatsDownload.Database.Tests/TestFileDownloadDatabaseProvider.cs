﻿namespace StatsDownload.Database.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using Core.Interfaces;
    using Core.Interfaces.DataTransfer;
    using Core.Interfaces.Enums;
    using Core.Interfaces.Logging;
    using NSubstitute;
    using NSubstitute.ClearExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class TestFileDownloadDatabaseProvider
    {
        [SetUp]
        public void SetUp()
        {
            databaseConnectionSettingsServiceMock = Substitute.For<IDatabaseConnectionSettingsService>();
            databaseConnectionSettingsServiceMock.GetConnectionString().Returns("connectionString");
            databaseConnectionSettingsServiceMock.GetCommandTimeout().Returns(42);

            databaseConnectionServiceMock = Substitute.For<IDatabaseConnectionService>();
            databaseConnectionServiceFactoryMock = Substitute.For<IDatabaseConnectionServiceFactory>();
            databaseConnectionServiceFactoryMock.Create("connectionString", 42).Returns(databaseConnectionServiceMock);

            loggingServiceMock = Substitute.For<ILoggingService>();

            errorMessageServiceMock = Substitute.For<IErrorMessageService>();

            systemUnderTest = NewFileDownloadDatabaseProvider(databaseConnectionSettingsServiceMock,
                databaseConnectionServiceFactoryMock, loggingServiceMock, errorMessageServiceMock);

            databaseConnectionServiceMock.CreateParameter(Arg.Any<string>(), Arg.Any<DbType>(),
                Arg.Any<ParameterDirection>()).Returns(info =>
            {
                var parameterName = info.Arg<string>();
                var dbType = info.Arg<DbType>();
                var direction = info.Arg<ParameterDirection>();

                var dbParameter = Substitute.For<DbParameter>();
                dbParameter.ParameterName.Returns(parameterName);
                dbParameter.DbType.Returns(dbType);
                dbParameter.Direction.Returns(direction);

                if (dbType.Equals(DbType.Int32))
                {
                    dbParameter.Value.Returns(default(int));
                }

                return dbParameter;
            });

            databaseConnectionServiceMock.CreateParameter(Arg.Any<string>(), Arg.Any<DbType>(),
                Arg.Any<ParameterDirection>(), Arg.Any<int>()).Returns(info =>
            {
                var parameterName = info.Arg<string>();
                var dbType = info.Arg<DbType>();
                var direction = info.Arg<ParameterDirection>();
                var size = info.Arg<int>();

                var dbParameter = Substitute.For<DbParameter>();
                dbParameter.ParameterName.Returns(parameterName);
                dbParameter.DbType.Returns(dbType);
                dbParameter.Direction.Returns(direction);
                dbParameter.Size.Returns(size);

                if (dbType.Equals(DbType.Int32))
                {
                    dbParameter.Value.Returns(default(int));
                }

                return dbParameter;
            });

            filePayload = new FilePayload
            {
                DownloadId = 100,
                DecompressedDownloadFileName = "DecompressedDownloadFileName",
                DecompressedDownloadFileExtension = "DecompressedDownloadFileExtension",
                DecompressedDownloadFileData = "DecompressedDownloadFileData"
            };

            fileDownloadResult = new FileDownloadResult(filePayload);

            dbDataReaderMock = Substitute.For<DbDataReader>();
            dbDataReaderMock.Read().Returns(true, true, true, false);
            dbDataReaderMock.GetInt32(0).Returns(100, 200, 300);

            databaseConnectionServiceMock
                .ExecuteReader("SELECT DownloadId FROM [FoldingCoin].[DownloadsReadyForUpload]")
                .Returns(dbDataReaderMock);
        }

        private const int NumberOfRowsEffectedExpected = 5;

        private IDatabaseConnectionServiceFactory databaseConnectionServiceFactoryMock;

        private IDatabaseConnectionService databaseConnectionServiceMock;

        private IDatabaseConnectionSettingsService databaseConnectionSettingsServiceMock;

        private DbDataReader dbDataReaderMock;

        private IErrorMessageService errorMessageServiceMock;

        private FileDownloadResult fileDownloadResult;

        private FilePayload filePayload;

        private ILoggingService loggingServiceMock;

        private IFileDownloadDatabaseService systemUnderTest;

        [Test]
        public void Constructor_WhenNullDependencyProvided_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewFileDownloadDatabaseProvider(null, databaseConnectionServiceFactoryMock, loggingServiceMock,
                        errorMessageServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewFileDownloadDatabaseProvider(databaseConnectionSettingsServiceMock, null, loggingServiceMock,
                        errorMessageServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewFileDownloadDatabaseProvider(databaseConnectionSettingsServiceMock,
                        databaseConnectionServiceFactoryMock, null, errorMessageServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewFileDownloadDatabaseProvider(databaseConnectionSettingsServiceMock,
                        databaseConnectionServiceFactoryMock, loggingServiceMock, null));
        }

        [Test]
        public void FileDownloadError_WhenInvoked_ParametersAreProvided()
        {
            errorMessageServiceMock.GetErrorMessage(FailedReason.UnexpectedException, filePayload,
                                       StatsDownloadService.FileDownload)
                                   .Returns("ErrorMessage");
            fileDownloadResult = new FileDownloadResult(FailedReason.UnexpectedException, filePayload);

            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure("[FoldingCoin].[FileDownloadError]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            InvokeFileDownloadError();

            Assert.That(actualParameters.Count, Is.EqualTo(2));
            Assert.That(actualParameters[0].ParameterName, Is.EqualTo("@DownloadId"));
            Assert.That(actualParameters[0].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[0].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[0].Value, Is.EqualTo(100));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@ErrorMessage"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo("ErrorMessage"));
        }

        [Test]
        public void FileDownloadError_WhenInvoked_UpdatesFileDownloadToError()
        {
            InvokeFileDownloadError();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("FileDownloadError Invoked");
                loggingServiceMock.LogVerbose("Database connection was successful");
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[FileDownloadError]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void FileDownloadFinished_WhenInvoked_FileDataUpload()
        {
            InvokeFileDownloadFinished();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("FileDownloadFinished Invoked");
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[FileDownloadFinished]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void FileDownloadFinished_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure("[FoldingCoin].[FileDownloadFinished]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            InvokeFileDownloadFinished();

            Assert.That(actualParameters.Count, Is.EqualTo(4));
            Assert.That(actualParameters[0].ParameterName, Is.EqualTo("@DownloadId"));
            Assert.That(actualParameters[0].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[0].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[0].Value, Is.EqualTo(100));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@FileName"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo("DecompressedDownloadFileName"));
            Assert.That(actualParameters[2].ParameterName, Is.EqualTo("@FileExtension"));
            Assert.That(actualParameters[2].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[2].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[2].Value, Is.EqualTo("DecompressedDownloadFileExtension"));
            Assert.That(actualParameters[3].ParameterName, Is.EqualTo("@FileData"));
            Assert.That(actualParameters[3].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[3].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[3].Value, Is.EqualTo("DecompressedDownloadFileData"));
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenInvoked_GetsLastfileDownloadDateTime()
        {
            InvokeGetLastFileFownloadDateTime();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("GetLastFileDownloadDateTime Invoked");
                databaseConnectionServiceMock.ExecuteScalar("SELECT [FoldingCoin].[GetLastFileDownloadDateTime]()");
            });
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenInvoked_ReturnsDateTime()
        {
            DateTime dateTime = DateTime.UtcNow;
            databaseConnectionServiceMock.ExecuteScalar("SELECT [FoldingCoin].[GetLastFileDownloadDateTime]()")
                                         .Returns(dateTime);

            DateTime actual = InvokeGetLastFileFownloadDateTime();

            Assert.That(actual, Is.EqualTo(dateTime));
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenNoRowsReturned_ReturnsDefaultDateTime()
        {
            DateTime actual = InvokeGetLastFileFownloadDateTime();

            Assert.That(actual, Is.EqualTo(default(DateTime)));
        }

        [Test]
        public void IsAvailable_WhenConnectionClosed_ConnectionOpened()
        {
            databaseConnectionServiceMock.ConnectionState.Returns(ConnectionState.Closed);

            InvokeIsAvailable();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("IsAvailable Invoked");
                databaseConnectionServiceMock.Open();
                loggingServiceMock.LogVerbose("Database connection was successful");
            });
        }

        [Test]
        public void IsAvailable_WhenConnectionOpen_ConnectionNotOpened()
        {
            databaseConnectionServiceMock.ConnectionState.Returns(ConnectionState.Open);

            InvokeIsAvailable();

            loggingServiceMock.DidNotReceive().LogVerbose("Database connection was successful");
            databaseConnectionServiceMock.DidNotReceive().Open();
        }

        [Test]
        public void IsAvailable_WhenDatabaseConnectionFails_LogsException()
        {
            var expected = new Exception();
            databaseConnectionServiceMock.When(mock => mock.Open()).Throw(expected);

            InvokeIsAvailable();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("IsAvailable Invoked");
                databaseConnectionServiceMock.Open();
                loggingServiceMock.LogException(expected);
            });
        }

        [Test]
        public void IsAvailable_WhenDatabaseConnectionFails_ReturnsFalse()
        {
            databaseConnectionServiceMock.When(mock => mock.Open()).Throw<Exception>();

            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsAvailable_WhenInvalidConnectionString_ReturnsFalse(string connectionString)
        {
            databaseConnectionSettingsServiceMock.GetConnectionString().Returns(connectionString);

            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsAvailable_WhenInvoked_ReturnsTrue()
        {
            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.True);
        }

        [Test]
        public void NewFileDownloadStarted_WhenEmptyString_ThrowsArgumentException()
        {
            databaseConnectionSettingsServiceMock.GetConnectionString().Returns(string.Empty);

            Assert.Throws<ArgumentException>(InvokeNewFileDownloadStarted);
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_NewFileDownloadStarted()
        {
            InvokeNewFileDownloadStarted();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("NewFileDownloadStarted Invoked");
                loggingServiceMock.LogVerbose("Database connection was successful");
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[NewFileDownloadStarted]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure(
                                                     "[FoldingCoin].[NewFileDownloadStarted]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            InvokeNewFileDownloadStarted();

            Assert.That(actualParameters.Count, Is.EqualTo(1));
            Assert.That(actualParameters[0].ParameterName, Is.EqualTo("@DownloadId"));
            Assert.That(actualParameters[0].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[0].Direction, Is.EqualTo(ParameterDirection.Output));
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_ReturnsDownloadId()
        {
            var dbParameter = Substitute.For<DbParameter>();
            dbParameter.Value.Returns(101);

            databaseConnectionServiceMock.ClearSubstitute();
            databaseConnectionServiceMock.CreateParameter("@DownloadId", DbType.Int32, ParameterDirection.Output)
                                         .Returns(dbParameter);

            InvokeNewFileDownloadStarted();

            Assert.That(filePayload.DownloadId, Is.EqualTo(101));
        }

        [Test]
        public void NewFileDownloadStarted_WhenNullConnectionString_ThrowsArgumentNullException()
        {
            databaseConnectionSettingsServiceMock.GetConnectionString().Returns((string) null);

            Assert.Throws<ArgumentNullException>(InvokeNewFileDownloadStarted);
        }

        [Test]
        public void UpdateToLatest_WhenInvoked_DatabaseUpdatedToLatest()
        {
            databaseConnectionServiceMock.ExecuteStoredProcedure(Arg.Any<string>())
                                         .Returns(NumberOfRowsEffectedExpected);

            InvokeUpdateToLatest();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogVerbose("UpdateToLatest Invoked");
                loggingServiceMock.LogVerbose("Database connection was successful");
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[UpdateToLatest]");
                loggingServiceMock.LogVerbose($"'{NumberOfRowsEffectedExpected}' rows were effected");
            });
        }

        private void InvokeFileDownloadError()
        {
            systemUnderTest.FileDownloadError(fileDownloadResult);
        }

        private void InvokeFileDownloadFinished()
        {
            systemUnderTest.FileDownloadFinished(filePayload);
        }

        private DateTime InvokeGetLastFileFownloadDateTime()
        {
            return systemUnderTest.GetLastFileDownloadDateTime();
        }

        private bool InvokeIsAvailable()
        {
            return systemUnderTest.IsAvailable();
        }

        private void InvokeNewFileDownloadStarted()
        {
            systemUnderTest.NewFileDownloadStarted(filePayload);
        }

        private void InvokeUpdateToLatest()
        {
            systemUnderTest.UpdateToLatest();
        }

        private IFileDownloadDatabaseService NewFileDownloadDatabaseProvider(
            IDatabaseConnectionSettingsService databaseConnectionSettingsService,
            IDatabaseConnectionServiceFactory databaseConnectionServiceFactory, ILoggingService loggingService,
            IErrorMessageService errorMessageService)
        {
            return new FileDownloadDatabaseProvider(databaseConnectionSettingsService,
                databaseConnectionServiceFactory,
                loggingService, errorMessageService);
        }

        private void SetUpDatabaseConnectionCreateDbCommandMock(
            Action<DbCommand>[] additionalCreateDbCommandSetupActions = null,
            Action<List<DbParameter>>[] additionalAddRangeSetUpActions = null)
        {
            var createCommandCallCount = 0;
            var addRangeCallCount = 0;

            databaseConnectionServiceMock.CreateDbCommand().Returns(createDbCommandInfo =>
            {
                var command = Substitute.For<DbCommand>();
                command.Parameters.Returns(parametersInfo =>
                {
                    var parameters = Substitute.For<DbParameterCollection>();
                    parameters.When(collection => collection.AddRange(Arg.Any<Array>())).Do(addRangeInfo =>
                    {
                        if (addRangeCallCount + 1 <= additionalAddRangeSetUpActions?.Length)
                        {
                            additionalAddRangeSetUpActions[addRangeCallCount]?.Invoke(
                                addRangeInfo.Arg<Array>().Cast<DbParameter>().ToList());
                            addRangeCallCount++;
                        }
                    });
                    return parameters;
                });

                if (createCommandCallCount + 1 <= additionalCreateDbCommandSetupActions?.Length)
                {
                    additionalCreateDbCommandSetupActions[createCommandCallCount]?.Invoke(command);
                    createCommandCallCount++;
                }

                return command;
            });
        }
    }
}