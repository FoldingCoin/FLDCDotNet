﻿namespace StatsDownload.Core
{
    public interface IFileDownloadMinimumWaitTimeService
    {
        bool IsMinimumWaitTimeMet(FilePayload filePayload);
    }
}