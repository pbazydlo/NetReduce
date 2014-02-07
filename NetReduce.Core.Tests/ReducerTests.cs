namespace NetReduce.Core.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Shouldly;
    using System;

    [TestClass]
    public class ReducerTests
    {
        private IStorage storage;

        [TestInitialize]
        public void Init()
        {
            this.storage = new InMemoryStorage();
        }

        [TestMethod]
        public void ReducerLoadsFilesAssociatedWithItsKey()
        {
            var keys = CreateTwoKeyFileSet(this.storage);

            var reducer = new Reducer(keys[0], null, this.storage);
            int loadedFileCount = reducer.LoadedFileCount;

            loadedFileCount.ShouldBe(3);
        }

        internal static string[] CreateTwoKeyFileSet(IStorage storage)
        {
            var keys = new string[] { "k1", "k2" };
            foreach (var key in keys)
            {
                for (int i = 0; i < 3; i++)
                {
                    var fileName = string.Format(Core.Properties.Settings.Default.MapOutputFileName, key, i, Guid.NewGuid());
                    storage.Store(fileName, "1");
                }
            }

            return keys;
        }

        [TestMethod]
        public void ReducerPerformsReduceOnLoadedFiles()
        {
            var keys = CreateTwoKeyFileSet(this.storage);
            var reducer = new Reducer(keys[0], (key, values) =>
            {
                int result = 0;
                foreach (var value in values)
                {
                    result += int.Parse(value);
                }

                return result.ToString();
            }, this.storage);

            var res = reducer.PerformReduce();

            res.ShouldBe("3");
        }

        [TestMethod]
        public void ReducerPerformsReduceOnLoadedFilesUsingExternalCode()
        {
            var keys = CreateTwoKeyFileSet(this.storage);

            TestHelpers.LoadToStorage(@"..\..\SampleReducer.cs", new Uri("file:///SampleReducer.cs"), this.storage);
            var reduceProvider = Loader.Load<IReduceProvider>("SampleReducer.cs", this.storage);
            var reducer = new Reducer(keys[0], reduceProvider.Reduce, this.storage);

            var res = reducer.PerformReduce();

            res.ShouldBe("3");
        }
    }
}
