namespace NetReduce.Core.Tests
{
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceMonitorTests
    {
        [Ignore]
        [TestMethod]
        public void PerformanceCounterTest()
        {
            var stop = false;
            var t = new Thread(
                () =>
                    {
                        while (!stop)
                        {
                        }
                    });
            t.Start();
            for (int i = 0; i < 60; i++)
            {
                if ((i+1) % 10 == 0)
                {
                    stop = !stop;
                }

                var value = 0;// PerformanceMonitor.GetProcessorTimeCounterValue();
                Debug.WriteLine(value);
            }

            stop = true;
            t.Join();
        }
    }
}
