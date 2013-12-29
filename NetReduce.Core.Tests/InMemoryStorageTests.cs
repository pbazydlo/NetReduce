namespace NetReduce.Core.Tests
{
    using System;
    using System.Linq;
    using Shouldly;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InMemoryStorageTests
    {
        [TestMethod]
        public void InMemoryStorageCleanMethodDeletesAllFiles()
        {
            var storage = new InMemoryStorage();
            storage.Store("a", "aa");
            storage.Store("b", "bb");

            storage.Clean();
            var noOfFiles = storage.ListFiles().Count();

            noOfFiles.ShouldBe(0);
        }
    }
}
