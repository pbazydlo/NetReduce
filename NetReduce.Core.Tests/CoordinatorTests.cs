namespace NetReduce.Core.Tests
{
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
        public void CoordinatorReadsKeysFromFileNames()
        {
            var keys = ReducerTests.CreateTwoKeyFileSet(this.storage);

            var coordinator = new Coordinator<ThreadWorker>(this.storage);
            var result = coordinator.GetKeys();

            result.Count().ShouldBe(keys.Length);
            foreach (var key in keys)
            {
                result.ShouldContain(key);
            }
        }

        [TestMethod]
        public void CoordinatorWorks()
        {
            this.storage.Store("f1", "ala ma kota");
            this.storage.Store("f2", "kota alama");
            this.storage.Store("f3", "dolan ma");
            var filesToRead = this.storage.ListFiles();
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", "SampleMapper.cs", this.storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", "SampleReducer.cs", this.storage);
            var coordinator = new Coordinator<ThreadWorker>(this.storage);

            coordinator.Start(2, 2, "SampleMapper.cs", "SampleReducer.cs", filesToRead);

            string result = string.Empty;
            foreach (var file in this.storage.ListFiles())
            {
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
            this.storage = new FileSystemStorage(@"c:\temp\netreduce", eraseContents: true);

            this.storage.Store("f1", "ala ma kota");
            this.storage.Store("f2", "kota alama");
            this.storage.Store("f3", "dolan ma");
            var filesToRead = this.storage.ListFiles();
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", "SampleMapper.cs", this.storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", "SampleReducer.cs", this.storage);
            var coordinator = new Coordinator<ThreadWorker>(this.storage);

            coordinator.Start(2, 2, "SampleMapper.cs", "SampleReducer.cs", filesToRead);

            string result = string.Empty;
            foreach (var file in this.storage.ListFiles())
            {
                if (file.Contains("REDUCE") && file.Contains("kota"))
                {
                    result = this.storage.Read(file);
                }
            }

            result.ShouldBe("2");
        }
    }
}
