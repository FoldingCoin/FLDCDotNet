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
    public class TestStatsUploadDatabaseProvider
    {
        [SetUp]
        public void SetUp()
        {
            databaseConnectionServiceMock = Substitute.For<IDatabaseConnectionService>();

            statsDownloadDatabaseServiceMock = Substitute.For<IStatsDownloadDatabaseService>();
            statsDownloadDatabaseServiceMock.When(service =>
                service.CreateDatabaseConnectionAndExecuteAction(
                    Arg.Any<Action<IDatabaseConnectionService>>())).Do(
                callInfo =>
                {
                    var service = callInfo.Arg<Action<IDatabaseConnectionService>>();

                    service.Invoke(databaseConnectionServiceMock);
                });

            loggingServiceMock = Substitute.For<ILoggingService>();

            errorMessageServiceMock = Substitute.For<IErrorMessageService>();

            systemUnderTest = NewStatsUploadDatabaseProvider(statsDownloadDatabaseServiceMock, loggingServiceMock,
                errorMessageServiceMock);

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

            dbDataReaderMock = Substitute.For<DbDataReader>();
            dbDataReaderMock.Read().Returns(true, true, true, false);
            dbDataReaderMock.GetInt32(0).Returns(100, 200, 300);

            databaseConnectionServiceMock
                .ExecuteReader("SELECT DownloadId FROM [FoldingCoin].[DownloadsReadyForUpload]")
                .Returns(dbDataReaderMock);

            downloadIdParameterMock = Substitute.For<DbParameter>();
            statsDownloadDatabaseServiceMock.CreateDownloadIdParameter(databaseConnectionServiceMock, 100)
                                            .Returns(downloadIdParameterMock);
            statsDownloadDatabaseServiceMock.CreateDownloadIdParameter(databaseConnectionServiceMock, 200)
                                            .Returns(downloadIdParameterMock);
            statsDownloadDatabaseServiceMock.CreateDownloadIdParameter(databaseConnectionServiceMock, 300)
                                            .Returns(downloadIdParameterMock);
            statsDownloadDatabaseServiceMock.CreateDownloadIdParameter(databaseConnectionServiceMock)
                                            .Returns(downloadIdParameterMock);

            errorMessageParameterMock = Substitute.For<DbParameter>();
            rejectionReasonParameterMock = Substitute.For<DbParameter>();
            statsDownloadDatabaseServiceMock.CreateRejectionReasonParameter(databaseConnectionServiceMock)
                                            .Returns(rejectionReasonParameterMock);
        }

        private IDatabaseConnectionService databaseConnectionServiceMock;

        private DbDataReader dbDataReaderMock;

        private DbParameter downloadIdParameterMock;

        private DbParameter errorMessageParameterMock;

        private IErrorMessageService errorMessageServiceMock;

        private ILoggingService loggingServiceMock;

        private DbParameter rejectionReasonParameterMock;

        private IStatsDownloadDatabaseService statsDownloadDatabaseServiceMock;

        private IStatsUploadDatabaseService systemUnderTest;

        [Test]
        public void AddUsers_WhenAddUserDataFails_AddsFailedUserToFailedList()
        {
            var dbParameter = Substitute.For<DbParameter>();
            dbParameter.Value.Returns(1);

            databaseConnectionServiceMock.ClearSubstitute();
            var command = Substitute.For<DbCommand>();
            command.Parameters.Returns(Substitute.For<DbParameterCollection>());
            databaseConnectionServiceMock.CreateDbCommand().Returns(command);
            databaseConnectionServiceMock.CreateParameter(Arg.Any<string>(), Arg.Any<DbType>(),
                ParameterDirection.Input).Returns(Substitute.For<DbParameter>());
            databaseConnectionServiceMock.CreateParameter("@ReturnValue", DbType.Int32, ParameterDirection.ReturnValue)
                                         .Returns(dbParameter);

            var user1 = new UserData(999, "name", 10, 100, 1000)
            {
                BitcoinAddress = "address",
                FriendlyName = "friendly"
            };
            var user2 = new UserData(900, "name", 10, 100, 1000)
            {
                BitcoinAddress = "address",
                FriendlyName = "friendly"
            };
            IList<FailedUserData> failedUsers = new List<FailedUserData>();

            systemUnderTest.AddUsers(null, 100,
                new List<UserData>
                {
                    user1,
                    user2
                }, failedUsers);

            Assert.That(failedUsers.Count, Is.EqualTo(2));
            Assert.That(failedUsers[0].RejectionReason, Is.EqualTo(RejectionReason.FailedAddToDatabase));
            Assert.That(failedUsers[0].UserData, Is.EqualTo(user1));
            Assert.That(failedUsers[0].LineNumber, Is.EqualTo(999));
            Assert.That(failedUsers[1].RejectionReason, Is.EqualTo(RejectionReason.FailedAddToDatabase));
            Assert.That(failedUsers[1].UserData, Is.EqualTo(user2));
            Assert.That(failedUsers[1].LineNumber, Is.EqualTo(900));
        }

        [Test]
        public void AddUsers_WhenInvoked_AddsUsers()
        {
            DbCommand failedUsersCommand = null;
            DbCommand addUsersCommand = null;
            DbCommand rebuildIndicesCommand = null;
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[]
            {
                dbCommand => failedUsersCommand = dbCommand,
                dbCommand => addUsersCommand = dbCommand,
                dbCommand => rebuildIndicesCommand = dbCommand
            });

            systemUnderTest.AddUsers(null, 1, new List<UserData> { new UserData(), new UserData(), new UserData() },
                new List<FailedUserData> { new FailedUserData(), new FailedUserData() });

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.AddUsers));
                failedUsersCommand.ExecuteNonQuery();
                failedUsersCommand.ExecuteNonQuery();
                addUsersCommand.ExecuteNonQuery();
                rebuildIndicesCommand.ExecuteNonQuery();
                addUsersCommand.ExecuteNonQuery();
                addUsersCommand.ExecuteNonQuery();
            });
        }

        [Test]
        public void AddUsers_WhenInvoked_AddUserDataParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            SetUpDatabaseConnectionCreateDbCommandMock(null,
                new Action<List<DbParameter>>[] { null, parameters => { actualParameters = parameters; } });

            systemUnderTest.AddUsers(null, 100,
                new List<UserData>
                {
                    new UserData(999, "name", 10, 100, 1000)
                    {
                        BitcoinAddress = "address",
                        FriendlyName = "friendly"
                    }
                }, null);

            Assert.That(actualParameters.Count, Is.EqualTo(9));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@LineNumber"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo(999));
            Assert.That(actualParameters[2].ParameterName, Is.EqualTo("@FAHUserName"));
            Assert.That(actualParameters[2].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[2].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[2].Value, Is.EqualTo("name"));
            Assert.That(actualParameters[3].ParameterName, Is.EqualTo("@TotalPoints"));
            Assert.That(actualParameters[3].DbType, Is.EqualTo(DbType.Int64));
            Assert.That(actualParameters[3].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[3].Value, Is.EqualTo(10));
            Assert.That(actualParameters[4].ParameterName, Is.EqualTo("@WorkUnits"));
            Assert.That(actualParameters[4].DbType, Is.EqualTo(DbType.Int64));
            Assert.That(actualParameters[4].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[4].Value, Is.EqualTo(100));
            Assert.That(actualParameters[5].ParameterName, Is.EqualTo("@TeamNumber"));
            Assert.That(actualParameters[5].DbType, Is.EqualTo(DbType.Int64));
            Assert.That(actualParameters[5].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[5].Value, Is.EqualTo(1000));
            Assert.That(actualParameters[6].ParameterName, Is.EqualTo("@FriendlyName"));
            Assert.That(actualParameters[6].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[6].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[6].Value, Is.EqualTo("friendly"));
            Assert.That(actualParameters[7].ParameterName, Is.EqualTo("@BitcoinAddress"));
            Assert.That(actualParameters[7].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[7].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[7].Value, Is.EqualTo("address"));
            Assert.That(actualParameters[8].ParameterName, Is.EqualTo("@ReturnValue"));
            Assert.That(actualParameters[8].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[8].Direction, Is.EqualTo(ParameterDirection.ReturnValue));
            Assert.That(actualParameters[8].Value, Is.EqualTo(0));
        }

        [Test]
        public void AddUsers_WhenInvoked_AddUserRejectionParametersAreProvided()
        {
            var failedUserData = new FailedUserData(10, "", RejectionReason.UnexpectedFormat, new UserData());
            errorMessageServiceMock.GetErrorMessage(failedUserData).Returns("RejectionReason");

            List<DbParameter> actualParameters = default(List<DbParameter>);
            SetUpDatabaseConnectionCreateDbCommandMock(null,
                new Action<List<DbParameter>>[] { parameters => { actualParameters = parameters; } });

            systemUnderTest.AddUsers(null, 100, null, new[] { failedUserData });

            Assert.That(actualParameters.Count, Is.EqualTo(3));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@LineNumber"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo(10));
            Assert.That(actualParameters[2], Is.EqualTo(rejectionReasonParameterMock));
            Assert.That(actualParameters[2].Value, Is.EqualTo("RejectionReason"));
        }

        [Test]
        public void AddUsers_WhenInvoked_DisposesCommands()
        {
            DbCommand failedUsersCommand = null;
            DbCommand addUsersCommand = null;
            DbCommand rebuildIndicesCommand = null;
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[]
            {
                dbCommand => failedUsersCommand = dbCommand,
                dbCommand => addUsersCommand = dbCommand,
                dbCommand => rebuildIndicesCommand = dbCommand
            });

            systemUnderTest.AddUsers(null, 1, new[] { new UserData(), new UserData() }, null);

            failedUsersCommand.Received(1).Dispose();
            addUsersCommand.Received(1).Dispose();
            rebuildIndicesCommand.Received(1).Dispose();
        }

        [Test]
        public void AddUsers_WhenInvoked_RebuildsIndicesPeriodically()
        {
            DbCommand command = null;
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[]
            {
                null,
                null,
                dbCommand => command = dbCommand
            });

            var users = new UserData[2501];
            for (var index = 0; index < users.Length; index++)
            {
                users[index] = new UserData();
            }

            systemUnderTest.AddUsers(null, 100, users, null);

            command.Received(2).ExecuteNonQuery();
        }

        [Test]
        public void AddUsers_WhenInvoked_ReusesCommands()
        {
            SetUpDatabaseConnectionCreateDbCommandMock();

            systemUnderTest.AddUsers(null, 100, null, null);

            databaseConnectionServiceMock.Received(3).CreateDbCommand();
        }

        [Test]
        public void AddUsers_WhenInvoked_ReusesParameters()
        {
            SetUpDatabaseConnectionCreateDbCommandMock();

            systemUnderTest.AddUsers(null, 100, null, null);

            databaseConnectionServiceMock.ReceivedWithAnyArgs(9)
                                         .CreateParameter(null, DbType.AnsiString, ParameterDirection.Input);
        }

        [Test]
        public void AddUsers_WhenInvoked_UsesAddUserDataStoredProcedure()
        {
            DbCommand command = default(DbCommand);
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[]
            {
                null,
                dbCommand => command = dbCommand
            });

            var transactionMock = Substitute.For<DbTransaction>();

            systemUnderTest.AddUsers(transactionMock, 1, null, null);

            command.Received(1).CommandText = "[FoldingCoin].[AddUserData]";
            command.Received(1).CommandType = CommandType.StoredProcedure;
            command.Received(1).Transaction = transactionMock;
        }

        [Test]
        public void AddUsers_WhenInvoked_UsesAddUserRejectionStoredProcedure()
        {
            DbCommand command = default(DbCommand);
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[] { dbCommand => command = dbCommand });

            var transactionMock = Substitute.For<DbTransaction>();

            systemUnderTest.AddUsers(transactionMock, 1, null, null);

            command.Received(1).CommandText = "[FoldingCoin].[AddUserRejection]";
            command.Received(1).CommandType = CommandType.StoredProcedure;
            command.Received(1).Transaction = transactionMock;
        }

        [Test]
        public void AddUsers_WhenInvoked_UsesRebuildsIndicesStoredProcedure()
        {
            DbCommand command = default(DbCommand);
            SetUpDatabaseConnectionCreateDbCommandMock(new Action<DbCommand>[]
            {
                null,
                null,
                dbCommand => command = dbCommand
            });

            var transactionMock = Substitute.For<DbTransaction>();

            systemUnderTest.AddUsers(transactionMock, 100, null, null);

            command.Received(1).CommandText = "[FoldingCoin].[RebuildIndices]";
            command.Received(1).CommandType = CommandType.StoredProcedure;
            command.Received(1).Transaction = transactionMock;
        }

        [Test]
        public void AddUsers_WhenInvokedWithNullBitcoinAddress_ParameterIsDBNull()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);
            SetUpDatabaseConnectionCreateDbCommandMock(null,
                new Action<List<DbParameter>>[] { null, parameters => { actualParameters = parameters; } });

            systemUnderTest.AddUsers(null, 100,
                new List<UserData> { new UserData(0, "name", 10, 100, 1000) { FriendlyName = "friendly" } }, null);

            Assert.That(actualParameters.Count, Is.EqualTo(9));
            Assert.That(actualParameters[7].ParameterName, Is.EqualTo("@BitcoinAddress"));
            Assert.That(actualParameters[7].Value, Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void AddUsers_WhenInvokedWithNullFriendlyName_ParameterIsDBNull()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);
            SetUpDatabaseConnectionCreateDbCommandMock(null,
                new Action<List<DbParameter>>[] { null, parameters => { actualParameters = parameters; } });

            systemUnderTest.AddUsers(null, 100,
                new List<UserData> { new UserData(0, "name", 10, 100, 1000) { BitcoinAddress = "address" } }, null);

            Assert.That(actualParameters.Count, Is.AtLeast(9));
            Assert.That(actualParameters[6].ParameterName, Is.EqualTo("@FriendlyName"));
            Assert.That(actualParameters[6].Value, Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void Commit_WhenInvoked_CommitsTransaction()
        {
            var transaction = Substitute.For<DbTransaction>();

            systemUnderTest.Commit(transaction);

            statsDownloadDatabaseServiceMock.Received().Commit(transaction);
        }

        [Test]
        public void Constructor_WhenNullDependencyProvided_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewStatsUploadDatabaseProvider(null, loggingServiceMock,
                        errorMessageServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                    NewStatsUploadDatabaseProvider(statsDownloadDatabaseServiceMock, null,
                        errorMessageServiceMock));
        }

        [Test]
        public void CreateTransaction_WhenInvoked_ReturnsTransaction()
        {
            var expected = Substitute.For<DbTransaction>();
            statsDownloadDatabaseServiceMock.CreateTransaction().Returns(expected);

            DbTransaction actual = systemUnderTest.CreateTransaction();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetDownloadsReadyForUpload_WhenInvoked_DisposesReader()
        {
            systemUnderTest.GetDownloadsReadyForUpload();

            dbDataReaderMock.Received().Dispose();
        }

        [Test]
        public void GetDownloadsReadyForUpload_WhenInvoked_GetsDownloadsReadyForUpload()
        {
            systemUnderTest.GetDownloadsReadyForUpload();

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.GetDownloadsReadyForUpload));
                databaseConnectionServiceMock.ExecuteReader(
                    "SELECT DownloadId FROM [FoldingCoin].[DownloadsReadyForUpload]");
            });
        }

        [Test]
        public void GetDownloadsReadyForUpload_WhenInvoked_ReturnsDownloadIds()
        {
            List<int> actual = systemUnderTest.GetDownloadsReadyForUpload().ToList();

            Assert.That(actual.Count, Is.EqualTo(3));
            Assert.That(actual[1], Is.EqualTo(200));
        }

        [Test]
        public void GetDownloadsReadyForUpload_WhenNoFilesReadyForUpload_ReturnsEmptyList()
        {
            dbDataReaderMock.ClearSubstitute();
            dbDataReaderMock.Read().Returns(false);

            List<int> actual = systemUnderTest.GetDownloadsReadyForUpload().ToList();

            Assert.That(actual.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetFileData_WhenInvoked_GetFileData()
        {
            systemUnderTest.GetFileData(100);

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.GetFileData));
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[GetFileData]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void GetFileData_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service => service.ExecuteStoredProcedure("[FoldingCoin].[GetFileData]",
                                                 Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            systemUnderTest.GetFileData(100);

            Assert.That(actualParameters.Count, Is.EqualTo(4));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@FileName"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Output));
            Assert.That(actualParameters[1].Size, Is.EqualTo(-1));
            Assert.That(actualParameters[2].ParameterName, Is.EqualTo("@FileExtension"));
            Assert.That(actualParameters[2].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[2].Direction, Is.EqualTo(ParameterDirection.Output));
            Assert.That(actualParameters[2].Size, Is.EqualTo(-1));
            Assert.That(actualParameters[3].ParameterName, Is.EqualTo("@FileData"));
            Assert.That(actualParameters[3].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[3].Direction, Is.EqualTo(ParameterDirection.Output));
            Assert.That(actualParameters[3].Size, Is.EqualTo(-1));
        }

        [Test]
        public void GetFileData_WhenInvoked_ReturnsFileData()
        {
            var dbParameter = Substitute.For<DbParameter>();
            dbParameter.Value.Returns("FileData");

            databaseConnectionServiceMock.ClearSubstitute();
            databaseConnectionServiceMock.CreateParameter("@FileData", DbType.String, ParameterDirection.Output, -1)
                                         .Returns(dbParameter);
            databaseConnectionServiceMock.CreateParameter("@DownloadId", DbType.Int32, ParameterDirection.Input)
                                         .Returns(Substitute.For<DbParameter>());

            string actual = systemUnderTest.GetFileData(100);

            Assert.That(actual, Is.EqualTo("FileData"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IsAvailable_WhenInvoked_ReturnsDatabaseAvailability(bool expected)
        {
            statsDownloadDatabaseServiceMock.IsAvailable().Returns(expected);

            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Rollback_WhenInvoked_RollsBackTransaction()
        {
            var transaction = Substitute.For<DbTransaction>();

            systemUnderTest.Rollback(transaction);

            statsDownloadDatabaseServiceMock.Received().Rollback(transaction);
        }

        [Test]
        public void StartStatsUpload_WhenInvoked_ParameterIsProvided()
        {
            var transaction = Substitute.For<DbTransaction>();

            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure(transaction,
                                                     "[FoldingCoin].[StartStatsUpload]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            DateTime dateTime = DateTime.UtcNow;

            systemUnderTest.StartStatsUpload(transaction, 100, dateTime);

            Assert.That(actualParameters.Count, Is.EqualTo(2));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@DownloadDateTime"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.DateTime));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo(dateTime));
        }

        [Test]
        public void StartStatsUpload_WhenInvoked_StartsStatsUpload()
        {
            var transaction = Substitute.For<DbTransaction>();

            systemUnderTest.StartStatsUpload(transaction, 1, DateTime.UtcNow);

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.StartStatsUpload));
                databaseConnectionServiceMock.ExecuteStoredProcedure(transaction, "[FoldingCoin].[StartStatsUpload]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void StatsUploadError_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure("[FoldingCoin].[StatsUploadError]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            var uploadResult = new StatsUploadResult(100, FailedReason.UnexpectedException);

            statsDownloadDatabaseServiceMock.CreateErrorMessageParameter(databaseConnectionServiceMock, uploadResult)
                                            .Returns(errorMessageParameterMock);

            systemUnderTest.StatsUploadError(uploadResult);

            Assert.That(actualParameters.Count, Is.EqualTo(2));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
            Assert.That(actualParameters[1], Is.EqualTo(errorMessageParameterMock));
        }

        [Test]
        public void StatsUploadError_WhenInvoked_UpdatesStatsUploadToError()
        {
            systemUnderTest.StatsUploadError(new StatsUploadResult());

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.StatsUploadError));
                databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[StatsUploadError]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        [Test]
        public void StatsUploadFinished_WhenInvoked_ParametersAreProvided()
        {
            var transaction = Substitute.For<DbTransaction>();

            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                                             service =>
                                                 service.ExecuteStoredProcedure(transaction,
                                                     "[FoldingCoin].[StatsUploadFinished]",
                                                     Arg.Any<List<DbParameter>>()))
                                         .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            systemUnderTest.StatsUploadFinished(transaction, 100);

            Assert.That(actualParameters.Count, Is.EqualTo(1));
            Assert.That(actualParameters[0], Is.EqualTo(downloadIdParameterMock));
        }

        [Test]
        public void StatsUploadFinished_WhenInvoked_UpdatesStatsUploadToFinished()
        {
            var transaction = Substitute.For<DbTransaction>();

            systemUnderTest.StatsUploadFinished(transaction, 100);

            Received.InOrder(() =>
            {
                loggingServiceMock.LogMethodInvoked(nameof(systemUnderTest.StatsUploadFinished));
                databaseConnectionServiceMock.ExecuteStoredProcedure(transaction, "[FoldingCoin].[StatsUploadFinished]",
                    Arg.Any<List<DbParameter>>());
            });
        }

        private bool InvokeIsAvailable()
        {
            return systemUnderTest.IsAvailable();
        }

        private IStatsUploadDatabaseService NewStatsUploadDatabaseProvider(
            IStatsDownloadDatabaseService statsDownloadDatabaseService, ILoggingService loggingService,
            IErrorMessageService errorMessageService)
        {
            return new StatsUploadDatabaseProvider(statsDownloadDatabaseService,
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