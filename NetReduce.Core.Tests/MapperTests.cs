namespace NetReduce.Core.Tests
{
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Shouldly;

    [TestClass]
    public class MapperTests
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
        public void MapperLoadsGivenSource()
        {
            var filePath = Path.Combine(Properties.Settings.Default.TestDirectory, "file1.txt");
            var fileContent = "whatever";
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(fileContent);
            }

            var mapper = new Mapper(filePath, null);

            mapper.Value.ShouldBe(fileContent);
        }

        [TestMethod]
        public void MapperPerformsMapOperation()
        {
            var filePath = Path.Combine(Properties.Settings.Default.TestDirectory, "file1.txt");
            var fileContent = "whatever am i";
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(fileContent);
            }

            var mapper = new Mapper(filePath, (key, value) => 
            {
                var result = new System.Collections.Generic.SortedList<string, string>();
                foreach (var w in value.Split(' '))
                    result.Add(w, "1");

                return result;
            });

            var mapResult = mapper.PerformMap();

            mapResult.Count().ShouldBe(3);
        }

        [TestMethod]
        public void MapperPerformsMapOperationUsingExternalCode()
        {
            var filePath = Path.Combine(Properties.Settings.Default.TestDirectory, "file1.txt");
            var fileContent = "whatever am i";
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(fileContent);
            }

            var mapProvider = Loader.Load<IMapProvider>(@"..\..\SampleMapper.cs");
            var mapper = new Mapper(filePath, mapProvider.Map);

            var mapResult = mapper.PerformMap();

            mapResult.Count().ShouldBe(3);
        }
    }
}
