﻿namespace StatsDownload.FileDownload.Console
{
    using System;
    using Core.Interfaces;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DependencyRegistration.Register();
                var service = WindsorContainer.Instance.Resolve<IFileDownloadService>();
                service.DownloadStatsFile();
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