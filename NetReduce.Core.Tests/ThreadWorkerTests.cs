namespace NetReduce.Core.Tests
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NetReduce.Core.Extensions;

    using Shouldly;

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
            var fileName = new Uri("file:///file1.txt");
            var fileContent = "whatever am i i";
            var mapperCodeFileName = new Uri("file:///SampleMapper.cs");

            storage.Store(storage.GetFileName(fileName), fileContent);
            TestHelpers.LoadToStorage(@"..\..\SampleMapper.cs", mapperCodeFileName, this.storage);
            var worker = new ThreadWorker(this.storage, 1);

            worker.Map(fileName, mapperCodeFileName);
            worker.Join();

            var fileNo = this.storage.ListFiles().Count();

            fileNo.ShouldBe(6);
        }

        [TestMethod]
        public void ThreadWorkerStoresReduceResults()
        {
            var reducerCodeFileName = new Uri("file:///SampleReducer.cs");

            var keys = ReducerTests.CreateTwoKeyFileSet(this.storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", reducerCodeFileName, this.storage);
            var worker = new ThreadWorker(this.storage, 1);

            worker.Reduce("k1", reducerCodeFileName);
            worker.Join();

            var result = default(string);
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = this.storage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = this.storage.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (key == "k1")
                    {
                        result = this.storage.Read(fileName);
                    }
                }
            }

            result.ShouldBe("3");
        }
    }
}
