namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;

    public class PerformanceMonitor
    {
        public static LoadStatistics GetLoadStatistics()
        {
            var result = new LoadStatistics();
            
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

        [DataContract]
        public class LoadStatistics
        {
            [DataMember]
            public float TotalProcessorTimeCounterPercent { get; set; }

            [DataMember]
            public float FreeRamCounterMB { get; set; }

            [DataMember]
            public float UsedRamCounterPercent { get; set; }
        }

        [DataContract]
        public class DriveStatistics
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public long TotalSize { get; set; }

            [DataMember]
            public long FreeSpace { get; set; }
        }

        [DataContract]
        public class PerformanceStatistics
        {
            [DataMember]
            public LoadStatistics LoadStatistics { get; set; }

            [DataMember]
            public DriveStatistics[] DriveStatistics { get; set; }
        }
    }
}
