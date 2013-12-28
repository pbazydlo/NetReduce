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
            var keys = CreateTwoKeyFileSet();

            var reducer = new Reducer(keys[0], testDirectory, null);
            int loadedFileCount = reducer.LoadedFileCount;

            loadedFileCount.ShouldBe(3);
        }

        private static string[] CreateTwoKeyFileSet()
        {
            var keys = new string[] { "k1", "k2" };
            foreach (var key in keys)
            {
                for (int i = 0; i < 3; i++)
                {
                    using (var writer = File.CreateText(Path.Combine(testDirectory, string.Format("{0}_MAP_NO_{1}", key, i))))
                    {
                        writer.Write("1");
                    }
                }
            }
            return keys;
        }

        [TestMethod]
        public void ReducerPerformsReduceOnLoadedFiles()
        {
            var keys = CreateTwoKeyFileSet();
            var reducer = new Reducer(keys[0], testDirectory, (key, values) =>
            {
                int result = 0;
                foreach (var value in values)
                {
                    result += int.Parse(value);
                }

                return result.ToString();
            });

            var res = reducer.PerformReduce();

            res.ShouldBe("3");
        }
    }
}
