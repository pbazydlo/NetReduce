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
            
            this.storage.Store(Base64.Encode("f1"), "ala ma kota");
            this.storage.Store(Base64.Encode("f2"), "kota alama");
            this.storage.Store(Base64.Encode("f3"), "dolan ma");
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
                if (file.Contains("REDUCE") && file.Contains(Base64.Encode("kota")))
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

            storage.Store(Base64.Encode("f1"), "ala ma kota");
            storage.Store(Base64.Encode("f2"), "kota alama");
            storage.Store(Base64.Encode("f3"), "dolan ma");
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
                if (file.Contains("REDUCE") && file.Contains(Base64.Encode("kota")))
                {
                    result = storage.Read(file);
                }
            }

            result.ShouldBe("2");
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
