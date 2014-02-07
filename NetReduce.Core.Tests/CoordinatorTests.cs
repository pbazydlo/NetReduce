namespace NetReduce.Core.Tests
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Shouldly;

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
        public void CoordinatorWorks()
        {
            this.storage.Store("f1", "ala ma kota");
            this.storage.Store("f2", "kota alama");
            this.storage.Store("f3", "dolan ma");
            var filesToRead = this.storage.ListFiles();
            var mapperCodeFile = new Uri("file:///SampleMapper.cs");
            var reducerCodeFile = new Uri("file:///SampleReducer.cs");
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", mapperCodeFile, this.storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", reducerCodeFile, this.storage);
            var coordinator = new Coordinator<ThreadWorker>(this.storage);

            coordinator.Start(2, 2, mapperCodeFile, reducerCodeFile, filesToRead);

            string result = string.Empty;
            foreach (var uri in this.storage.ListFiles())
            {
                var file = this.storage.GetFileName(uri);
                if (file.Contains("REDUCE") && file.Contains("kota"))
                {
                    result = this.storage.Read(file);
                }
            }

            result.ShouldBe("2");
        }

        [TestMethod]
        public void CoordinatorWorksOnFileSystemStorage()
        {
            var storage = new FileSystemStorage(@"c:\temp\netreduce", eraseContents: true) as IStorage;

            storage.Store("f1", "ala ma kota");
            storage.Store("f2", "kota alama");
            storage.Store("f3", "dolan ma");
            var filesToRead = storage.ListFiles();
            var mapperCodeFile = new Uri("file:///SampleMapper.cs");
            var reducerCodeFile = new Uri("file:///SampleReducer.cs");
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", mapperCodeFile, storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", reducerCodeFile, storage);
            var coordinator = new Coordinator<ThreadWorker>(storage);

            coordinator.Start(2, 2, mapperCodeFile, reducerCodeFile, filesToRead);

            string result = string.Empty;
            foreach (var uri in storage.ListFiles())
            {
                var file = this.storage.GetFileName(uri);
                if (file.Contains("REDUCE") && file.Contains("kota"))
                {
                    result = storage.Read(file);
                }
            }

            result.ShouldBe("2");
        }
    }
}
