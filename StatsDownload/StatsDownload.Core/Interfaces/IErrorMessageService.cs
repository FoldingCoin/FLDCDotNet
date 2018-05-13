﻿namespace StatsDownload.Core.Interfaces
{
    using System.Collections.Generic;
    using Interfaces.DataTransfer;
    using Interfaces.Enums;

    using StatsDownload.Core.DataTransfer;

    public interface IErrorMessageService
    {
        string GetErrorMessage(FailedReason failedReason, FilePayload filePayload);

        string GetErrorMessage(FailedReason failedReason);

        string GetErrorMessage(IEnumerable<FailedUserData> failedUsersData);

        string GetErrorMessage(FailedUserData failedUserData);
    }
}