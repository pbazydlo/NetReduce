using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;

namespace NetReduce.Core.Tests
{
    [TestClass]
    public class ReducerTests
    {
        private const string testDirectory = "./tests";

        [TestInitialize]
        public void Init()
        {
            TestHelpers.ClearAndCreateDirectory(testDirectory);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            TestHelpers.ClearAndCreateDirectory(testDirectory);
        }

        [TestMethod]
        public void ReducerLoadsFilesAssociatedWithItsKey()
        {
            var keys = new string[] { "k1", "k2" };
            foreach (var key in keys)
            {
                for (int i = 0; i < 3; i++)
                {
                    using (var writer = File.CreateText(Path.Combine(testDirectory, string.Format("{0}_MAP_NO_{1}", key, i))))
                    {
                        writer.Write(key);
                    }
                }
            }

            var reducer = new Reducer(keys[0], testDirectory);
            int loadedFileCount = reducer.LoadedFileCount;

            loadedFileCount.ShouldBe(3);
        }
    }
}
