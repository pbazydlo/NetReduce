namespace NetReduce.Core
{
    using System.Diagnostics;

    public class PerformanceMonitor
    {
        public static float GetProcessorTimeCounterValue()
        {
            PerformanceCounter totalProcessorTimeCounter = new PerformanceCounter(
             "Process",
             "% Processor Time",
             /*"_Total"*/ Process.GetCurrentProcess().ProcessName);
            totalProcessorTimeCounter.NextValue();
            System.Threading.Thread.Sleep(1000);// 1 second wait
            var value = totalProcessorTimeCounter.NextValue();

            return value;
        }
    }
}
