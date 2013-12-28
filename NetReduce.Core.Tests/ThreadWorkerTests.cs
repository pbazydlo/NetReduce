using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace NetReduce.Core.Tests
{
    using System.Text.RegularExpressions;

    using NetReduce.Core.Extensions;

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

        [TestMethod]
        public void ThreadWorkerStoresReduceResults()
        {
            var keys = ReducerTests.CreateTwoKeyFileSet(this.storage);
            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", "SampleReducer.cs", this.storage);
            var worker = new ThreadWorker(this.storage, 1);

            worker.Reduce("k1", "SampleReducer.cs");
            worker.Join();

            var result = default (string);
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString)); //[^<>:""\\/|\?\*]
            var fileNames = this.storage.ListFiles();
            foreach (var fileName in fileNames)
            {
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
