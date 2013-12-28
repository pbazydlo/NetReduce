using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using NetReduce.Core;
using System.IO;
using System.Collections.Generic;

namespace NetReduce.Core.Tests
{
    using System.Linq;

    [TestClass]
    public class MapperTests
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
        public void MapperLoadsGivenSource()
        {
            var filePath = Path.Combine(testDirectory, "file1.txt");
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
            var filePath = Path.Combine(testDirectory, "file1.txt");
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
            var filePath = Path.Combine(testDirectory, "file1.txt");
            var fileContent = "whatever am i";
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(fileContent);
            }

            var mapFunc = Loader.Load<IMapProvider>(@"..\..\SampleMapper.cs");
            var mapper = new Mapper(filePath, mapFunc.Map);

            var mapResult = mapper.PerformMap();

            mapResult.Count().ShouldBe(3);
        }
    }
}
