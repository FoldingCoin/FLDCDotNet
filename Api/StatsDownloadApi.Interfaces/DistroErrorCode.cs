﻿namespace StatsDownloadApi.Interfaces
{
    public enum DistroErrorCode
    {
        None = 0000,

        NoStartDate = 1000,

        NoEndDate = 1010,

        StartDateUnsearchable = 1020,

        EndDateUnsearchable = 1030,

        InvalidDateRange = 1040,

        DatabaseUnavailable = 8000
    }
}