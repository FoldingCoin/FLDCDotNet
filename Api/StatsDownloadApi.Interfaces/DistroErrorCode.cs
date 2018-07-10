﻿namespace StatsDownloadApi.Interfaces
{
    public enum DistroErrorCode
    {
        None = 0000,

        StartDateInvalid = 1000,

        EndDateInvalid = 1010,

        DatabaseUnavailable = 8000
    }
}