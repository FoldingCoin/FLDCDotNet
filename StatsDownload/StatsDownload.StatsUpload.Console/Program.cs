﻿namespace StatsDownload.StatsUpload.Console
{
    using System;

    using StatsDownload.Core.Interfaces;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DependencyRegistration.Register();
                var service = WindsorContainer.Instance.Resolve<IStatsUploadService>();
                service.UploadStatsFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                WindsorContainer.Dispose();
                Console.WriteLine(new string('-', 100));
                Console.WriteLine();
            }
        }
    }
}