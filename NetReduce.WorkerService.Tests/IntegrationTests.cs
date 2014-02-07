using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using NetReduce.Core;
using NetReduce.Core.Tests;
using NetReduce.Remote;
using System.Diagnostics;
using System.Threading;

namespace NetReduce.WorkerService.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        // TODO: These tests require running NetReduce.WorkerService
        // on the same machine as these tests ;)

        [TestInitialize]
        public void Init()
        {
            this.storage = new FileSystemStorage(this.storagePath, eraseContents: true);
            this.serviceProcess = Process.Start(new ProcessStartInfo(@"..\..\..\NetReduce.WorkerService.ConsoleHost\bin\Debug\NetReduce.WorkerService.ConsoleHost.exe"));
            Thread.Sleep(100);
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.serviceProcess.Kill();
        }

        // For now only for map.
        [TestMethod]
        public void RemoteMapWorks()
        {
            var fileName = "file1.txt";
            var fileContent = "whatever am i i";
            var mapperCodeFileName = "SampleMapper.cs";

            this.storage.Store(fileName, fileContent);
            TestHelpers.LoadToStorage(@"..\..\..\NetReduce.Core.Tests\SampleMapper.cs", mapperCodeFileName, this.storage);

            var worker = new RemoteWorker<ServiceClientWrapper>();

            worker.Map(fileName, mapperCodeFileName);
            worker.Join();

            var fileNo = this.storage.ListFiles().Count();

            fileNo.ShouldBe(6);
        }

        private string storagePath = @"c:\temp\netreduce";
        private IStorage storage;
        private Process serviceProcess;
    }
}
