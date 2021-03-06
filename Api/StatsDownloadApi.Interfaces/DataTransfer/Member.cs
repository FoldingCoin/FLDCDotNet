﻿namespace StatsDownloadApi.Interfaces.DataTransfer
{
    public class Member
    {
        public Member(string userName, string friendlyName, string bitcoinAddress, long teamNumber, long startPoints,
                      long startWorkUnits, long pointsGained, long workUnitsGained)
        {
            UserName = userName;
            FriendlyName = friendlyName;
            BitcoinAddress = bitcoinAddress;
            TeamNumber = teamNumber;
            StartPoints = startPoints;
            StartWorkUnits = startWorkUnits;
            PointsGained = pointsGained;
            WorkUnitsGained = workUnitsGained;
        }

        public string BitcoinAddress { get; }

        public string FriendlyName { get; }

        public long PointsGained { get; }

        public long StartPoints { get; }

        public long StartWorkUnits { get; }

        public long TeamNumber { get; }

        public string UserName { get; }

        public long WorkUnitsGained { get; }
    }
}