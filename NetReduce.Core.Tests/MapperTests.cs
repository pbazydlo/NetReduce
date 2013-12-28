using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using NetReduce.Core;
using System.IO;

namespace NetReduce.Core.Tests
{
    [TestClass]
    public class MapperTests
    {
        private const string testDirectory = "./tests";

        [TestInitialize]
        public void Init()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }

            Directory.CreateDirectory(testDirectory);
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

            var mapper = new Mapper(filePath);

            mapper.Source.ShouldBe(fileContent);
        }
    }
}
