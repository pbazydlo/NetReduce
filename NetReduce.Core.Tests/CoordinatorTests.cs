using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetReduce.Core;
using Shouldly;

namespace NetReduce.Core.Tests
{
    using System.Linq;

    [TestClass]
    public class CoordinatorTests
    {
        private IStorage storage;

        [TestInitialize]
        public void Init()
        {
            this.storage = new InMemoryStorage();
        }

        [TestMethod]
        public void CoordinatorSpawnsReducersAfterMappers()
        {
            object mapLock = new object();
            object reduceLock = new object();
            int mapAdds = 0;
            int reduceAdds = 0;
            var coordinator = new Coordinator(map: () => { lock (mapLock) { mapAdds++; } }, 
                reduce: () => { lock (reduceLock) { reduceAdds += mapAdds; } }, storage: this.storage);

            coordinator.Start(maxMapperNo: 5, maxReducerNo: 2);

            mapAdds.ShouldBe(5);
            reduceAdds.ShouldBe(10);
        }

        [TestMethod]
        public void CoordinatorReadsKeysFromFileNames()
        {
            var keys = ReducerTests.CreateTwoKeyFileSet(this.storage);

            var coordinator = new Coordinator(null, null, this.storage);
            var result = coordinator.GetKeys();

            result.Count().ShouldBe(keys.Length);
            foreach (var key in keys)
            {
                result.ShouldContain(key);
            }
        }
    }
}
