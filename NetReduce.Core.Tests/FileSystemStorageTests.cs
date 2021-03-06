﻿namespace NetReduce.Core.Tests
{
    using System;
    using System.Linq;
    using Shouldly;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileSystemStorageTests
    {
        [TestMethod]
        public void FileSystemStoragCleanMethodDeletesAllFiles()
        {
            var storage = new FileSystemStorage(@"c:\temp\netreduce", eraseContents: true);
            storage.Store("a", "aa");
            storage.Store("b", "bb");

            storage.Clean();
            var noOfFiles = storage.ListFiles().Count();

            noOfFiles.ShouldBe(0);
        }

        [TestMethod]
        public void FileSystemStorageRemoveDeletesGivenFile()
        {
            var storage = new FileSystemStorage(@"c:\temp\netreduce", eraseContents: true);
            storage.Store("a", "aa");
            storage.Store("b", "bb");
            var fileToRemove = storage.ListFiles().First(u => u.OriginalString.Contains("a"));

            storage.Remove(fileToRemove);

            var noOfFiles = storage.ListFiles().Count();
            noOfFiles.ShouldBe(1);
            storage.ListFiles().First().OriginalString.ShouldContain("b");
        }
    }
}
