using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace NetReduce.Core.Tests
{
    [TestClass]
    public class ThreadWorkerTests
    {
        private IStorage storage;

        [TestInitialize]
        public void Init()
        {
            this.storage = new InMemoryStorage();
        }

        [TestMethod]
        public void ThreadWorkerStoresMapResults()
        {
            var fileName = "file1.txt";
            var fileContent = "whatever am i i";
            storage.Store(fileName, fileContent);
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", "SampleMapper.cs", this.storage);
            var worker = new ThreadWorker(this.storage, 1);

            worker.Map(fileName, "SampleMapper.cs");
            worker.Join();

            var fileNo = this.storage.ListFiles().Count();

            fileNo.ShouldBe(6);
        }
    }
}
