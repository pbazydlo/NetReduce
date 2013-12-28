namespace NetReduce.Core.Tests
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Shouldly;

    [TestClass]
    public class ReducerTests
    {
        [TestInitialize]
        public void Init()
        {
            TestHelpers.ClearAndCreateDirectory(Properties.Settings.Default.TestDirectory);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            TestHelpers.ClearAndCreateDirectory(Properties.Settings.Default.TestDirectory);
        }

        [TestMethod]
        public void ReducerLoadsFilesAssociatedWithItsKey()
        {
            var keys = CreateTwoKeyFileSet(Properties.Settings.Default.TestDirectory);

            var reducer = new Reducer(keys[0], Properties.Settings.Default.TestDirectory, null);
            int loadedFileCount = reducer.LoadedFileCount;

            loadedFileCount.ShouldBe(3);
        }

        internal static string[] CreateTwoKeyFileSet(string testDirectory)
        {
            var keys = new string[] { "k1", "k2" };
            foreach (var key in keys)
            {
                for (int i = 0; i < 3; i++)
                {
                    using (var writer = File.CreateText(Path.Combine(testDirectory, string.Format(Core.Properties.Settings.Default.MapOutputFileName, key, i))))
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
            var keys = CreateTwoKeyFileSet(Properties.Settings.Default.TestDirectory);
            var reducer = new Reducer(keys[0], Properties.Settings.Default.TestDirectory, (key, values) =>
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

        [TestMethod]
        public void ReducerPerformsReduceOnLoadedFilesUsingExternalCode()
        {
            var keys = CreateTwoKeyFileSet(Properties.Settings.Default.TestDirectory);

            var reduceProvider = Loader.Load<IReduceProvider>(@"..\..\SampleReducer.cs");
            var reducer = new Reducer(keys[0], Properties.Settings.Default.TestDirectory, reduceProvider.Reduce);

            var res = reducer.PerformReduce();

            res.ShouldBe("3");
        }
    }
}
