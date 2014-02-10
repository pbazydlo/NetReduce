namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public class PerformanceMonitor
    {
        public static PerformanceStatistics GetPerformanceStatistics()
        {
            var result = new PerformanceStatistics();
            
            var totalProcessorTimeCounterPercent = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var freeRamCounterMB = new PerformanceCounter("Memory", "Available MBytes");
            var usedRamCounterPercent = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            totalProcessorTimeCounterPercent.NextValue();
            freeRamCounterMB.NextValue();
            usedRamCounterPercent.NextValue();

            System.Threading.Thread.Sleep(1000); // 1 second wait

            result.TotalProcessorTimeCounterPercent = totalProcessorTimeCounterPercent.NextValue();
            result.FreeRamCounterMB = freeRamCounterMB.NextValue();
            result.UsedRamCounterPercent = usedRamCounterPercent.NextValue();

            return result;
        }

        public static List<DriveStatistics> GetHddStatistics(string driveName = null)
        {
            var result = new List<DriveStatistics>();

            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (!string.IsNullOrEmpty(driveName) && !drive.Name.ToUpper().StartsWith(driveName.ToUpper()))
                {
                    continue;
                }

                var driveStats = new DriveStatistics();

                driveStats.Name = drive.Name;
                driveStats.TotalSize = drive.TotalSize;
                driveStats.FreeSpace = drive.TotalFreeSpace;

                result.Add(driveStats);
            }

            return result;
        }

        public class PerformanceStatistics
        {
            public float TotalProcessorTimeCounterPercent { get; set; }
            public float FreeRamCounterMB { get; set; }
            public float UsedRamCounterPercent { get; set; }
        }

        public class DriveStatistics
        {
            public string Name { get; set; }
            public long TotalSize { get; set; }
            public long FreeSpace { get; set; }
        }
    }
}
