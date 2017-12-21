﻿namespace StatsDownload.Core
{
    using System;

    public class FileDownloadProvider : IFileDownloadService
    {
        private readonly IFileDownloadDataStoreService fileDownloadDataStoreService;

        private readonly IFileDownloaderService fileDownloaderService;

        private readonly IFileDownloadLoggingService fileDownloadLoggingService;

        private readonly IFileDownloadSettingsService fileDownloadSettingsService;

        private readonly IFileDownloadTimeoutValidatorService fileDownloadTimeoutValidatorService;

        private readonly IFileNameService fileNameService;

        public FileDownloadProvider(
            IFileDownloadDataStoreService fileDownloadDataStoreService,
            IFileDownloadLoggingService fileDownloadLoggingService,
            IFileDownloadSettingsService fileDownloadSettingsService,
            IFileDownloaderService fileDownloaderService,
            IFileDownloadTimeoutValidatorService fileDownloadTimeoutValidatorService,
            IFileNameService fileNameService)
        {
            if (IsNull(fileDownloadDataStoreService))
            {
                throw NewArgumentNullException(nameof(fileDownloadDataStoreService));
            }

            if (IsNull(fileDownloadLoggingService))
            {
                throw NewArgumentNullException(nameof(fileDownloadLoggingService));
            }

            if (IsNull(fileDownloadSettingsService))
            {
                throw NewArgumentNullException(nameof(fileDownloadSettingsService));
            }

            if (IsNull(fileDownloaderService))
            {
                throw NewArgumentNullException(nameof(fileDownloaderService));
            }

            if (IsNull(fileDownloadTimeoutValidatorService))
            {
                throw NewArgumentNullException(nameof(fileDownloadTimeoutValidatorService));
            }

            if (IsNull(fileNameService))
            {
                throw NewArgumentNullException(nameof(fileNameService));
            }

            this.fileDownloadDataStoreService = fileDownloadDataStoreService;
            this.fileDownloadLoggingService = fileDownloadLoggingService;
            this.fileDownloadSettingsService = fileDownloadSettingsService;
            this.fileDownloaderService = fileDownloaderService;
            this.fileDownloadTimeoutValidatorService = fileDownloadTimeoutValidatorService;
            this.fileNameService = fileNameService;
        }

        public FileDownloadResult DownloadStatsFile()
        {
            try
            {
                LogMethodInvoked(nameof(DownloadStatsFile));

                if (DataStoreUnavailable())
                {
                    FileDownloadResult failedResult = NewFailedFileDownloadResult(FailedReason.DataStoreUnavailable);
                    LogResult(failedResult);
                    return failedResult;
                }

                UpdateToLatest();

                int downloadId = NewFileDownloadStarted();

                string downloadUrl = GetDownloadUrl();
                string downloadTimeout = GetDownloadTimeout();
                string downloadFileName = GetDownloadFileName();

                int timeoutInSeconds;
                TryParseTimeout(downloadTimeout, out timeoutInSeconds);

                DownloadFile(downloadUrl, downloadFileName, timeoutInSeconds);

                FileDownloadResult successResult = NewSuccessFileDownloadResult(
                    downloadId,
                    downloadUrl,
                    downloadTimeout,
                    downloadFileName);
                LogResult(successResult);

                return successResult;
            }
            catch (Exception exception)
            {
                FileDownloadResult result = NewFailedFileDownloadResult(FailedReason.UnexpectedException);
                LogResult(result);
                LogException(exception);
                return result;
            }
        }

        private bool DataStoreUnavailable()
        {
            return !fileDownloadDataStoreService.IsAvailable();
        }

        private void DownloadFile(string downloadUrl, string fileName, int timeoutInSeconds)
        {
            fileDownloaderService.DownloadFile(downloadUrl, fileName, timeoutInSeconds);
        }

        private string GetDownloadDirectory()
        {
            return fileDownloadSettingsService.GetDownloadDirectory();
        }

        private string GetDownloadFileName()
        {
            string downloadDirectory = GetDownloadDirectory();
            return fileNameService.GetRandomFileNamePath(downloadDirectory);
        }

        private string GetDownloadTimeout()
        {
            return fileDownloadSettingsService.GetDownloadTimeout();
        }

        private string GetDownloadUrl()
        {
            return fileDownloadSettingsService.GetDownloadUrl();
        }

        private bool IsNull(object value)
        {
            return value == null;
        }

        private void LogException(Exception exception)
        {
            fileDownloadLoggingService.LogException(exception);
        }

        private void LogMethodInvoked(string method)
        {
            LogVerbose($"{method} Invoked");
        }

        private void LogResult(FileDownloadResult result)
        {
            fileDownloadLoggingService.LogResult(result);
        }

        private void LogVerbose(string message)
        {
            fileDownloadLoggingService.LogVerbose(message);
        }

        private Exception NewArgumentNullException(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        private FileDownloadResult NewFailedFileDownloadResult(FailedReason failedReason)
        {
            return new FileDownloadResult(failedReason);
        }

        private int NewFileDownloadStarted()
        {
            return fileDownloadDataStoreService.NewFileDownloadStarted();
        }

        private FileDownloadResult NewSuccessFileDownloadResult(
            int downloadId,
            string downloadUrl,
            string downloadTimeout,
            string downloadDirectory)
        {
            return new FileDownloadResult(downloadId, downloadUrl, downloadTimeout, downloadDirectory);
        }

        private bool TryParseTimeout(string unsafeTimeout, out int timeoutInSeconds)
        {
            return fileDownloadTimeoutValidatorService.TryParseTimeout(unsafeTimeout, out timeoutInSeconds);
        }

        private void UpdateToLatest()
        {
            fileDownloadDataStoreService.UpdateToLatest();
        }
    }
}