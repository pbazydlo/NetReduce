using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetReduce.Core;
using Shouldly;

namespace NetReduce.Core.Tests
{
    [TestClass]
    public class CoordinatorTests
    {
        [TestMethod]
        public void CoordinatorSpawnsReducersAfterMappers()
        {
            object mapLock = new object();
            object reduceLock = new object();
            int mapAdds = 0;
            int reduceAdds = 0;
            var coordinator = new Coordinator(map: () => { lock (mapLock) { mapAdds++; } }, reduce: () => { lock (reduceLock) { reduceAdds += mapAdds; } });

            coordinator.Start(maxMapperNo: 5, maxReducerNo: 2);

            mapAdds.ShouldBe(5);
            reduceAdds.ShouldBe(10);
        }
    }
}
