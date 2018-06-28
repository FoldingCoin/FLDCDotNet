﻿namespace StatsDownload.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using DataTransfer;

    public interface IStatsUploadDatabaseService
    {
        void AddUsers(DbTransaction transaction, int downloadId, IEnumerable<UserData> usersData,
            IEnumerable<FailedUserData> failedUsers);

        void Commit(DbTransaction transaction);

        IEnumerable<int> GetDownloadsReadyForUpload();

        string GetFileData(int downloadId);

        bool IsAvailable();

        void Rollback(DbTransaction transaction);

        DbTransaction StartStatsUpload(int downloadId);

        void StatsUploadError(StatsUploadResult statsUploadResult);

        void StatsUploadFinished(DbTransaction transaction, int downloadId, DateTime downloadDateTime);
    }
}