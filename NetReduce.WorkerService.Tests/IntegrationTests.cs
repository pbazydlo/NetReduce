using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using NetReduce.Core;
using NetReduce.Core.Tests;
using NetReduce.Remote;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using NetReduce.Core.Extensions;

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
            var fileName = new Uri("file:///file1.txt");
            var fileContent = "whatever am i i";
            var mapperCodeFileName = new Uri("file:///SampleMapper.cs");
            this.storage.Store(fileName, fileContent);
            TestHelpers.LoadToStorage(@"..\..\..\NetReduce.Core.Tests\SampleMapper.cs", mapperCodeFileName, this.storage);
            IUriProvider uriProvider = new UriProvider();
            uriProvider.Uris.Add(new Uri("http://localhost:28756/RemoteWorkerService.svc"));
            ServiceClientWrapper.UriProvider = uriProvider;
            var worker = new RemoteWorker<ServiceClientWrapper>();
            worker.Storage = this.storage;
            worker.Init();

            worker.Map(fileName, mapperCodeFileName);
            worker.Join();

            var workerStorage = new FileSystemStorage(@"c:\temp\netreduce\0", false);
            var fileNo = workerStorage.ListFiles().Count();
            fileNo.ShouldBe(6);
        }

        [TestMethod]
        public void RemoteReduceWorks()
        {
            var workerStorage = new FileSystemStorage(@"c:\temp\netreduce\0", false);
            var reducerCodeFileName = new Uri("file:///SampleReducer.cs");
            TestHelpers.LoadToStorage(@"..\..\..\NetReduce.Core.Tests\SampleReducer.cs", reducerCodeFileName, this.storage);
            IUriProvider uriProvider = new UriProvider();
            uriProvider.Uris.Add(new Uri("http://localhost:28756/RemoteWorkerService.svc"));
            ServiceClientWrapper.UriProvider = uriProvider;
            var worker = new RemoteWorker<ServiceClientWrapper>();
            worker.Storage = this.storage;
            worker.Init();

            // Init reducer storage after it is created (cleaning of directory upon worker creation in service)
            var keys = ReducerTests.CreateTwoKeyFileSet(workerStorage);

            worker.Reduce("k1", reducerCodeFileName);
            worker.Join();

            var result = default(string);
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = workerStorage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = workerStorage.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (key == "k1")
                    {
                        result = workerStorage.Read(fileName);
                    }
                }
            }

            result.ShouldBe("3");
        }

        [TestMethod]
        public void RemoteCoordinatorWorks()
        {
            this.storage.Store("f1", "ala ma kota");
            this.storage.Store("f2", "kota alama");
            this.storage.Store("f3", "dolan ma");
            var filesToRead = this.storage.ListFiles();
            var mapperCodeFile = new Uri("file:///SampleMapper.cs");
            var reducerCodeFile = new Uri("file:///SampleReducer.cs");
            TestHelpers.LoadToStorage(@"..\..\..\NetReduce.Core.Tests\SampleMapper.cs", mapperCodeFile, this.storage);
            TestHelpers.LoadToStorage(@"..\..\..\NetReduce.Core.Tests\SampleReducer.cs", reducerCodeFile, this.storage);
            IUriProvider uriProvider = new UriProvider();
            uriProvider.Uris.Add(new Uri("http://localhost:28756/RemoteWorkerService.svc"));
            ServiceClientWrapper.UriProvider = uriProvider;
            var coordinator = new Coordinator<RemoteWorker<ServiceClientWrapper>>(this.storage);

            coordinator.Start(2, 2, mapperCodeFile, reducerCodeFile, filesToRead);

            var reduceStorage3 = new FileSystemStorage(@"c:\temp\netreduce\3", false);
            var reduceStorage4 = new FileSystemStorage(@"c:\temp\netreduce\4", false);
            var storages = new[] { reduceStorage3, reduceStorage4 };
            string result = string.Empty;
            foreach (var reduceStorage in storages)
            {
                foreach (var uri in reduceStorage.ListFiles())
                {
                    var file = reduceStorage.GetFileName(uri);
                    if (file.Contains("REDUCE") && file.Contains("kota"))
                    {
                        result = reduceStorage.Read(file);
                    }
                }
            }

            result.ShouldBe("2");
        }

        private string storagePath = @"c:\temp\netreduce";
        private IStorage storage;
        private Process serviceProcess;

        [TestMethod]
        public void ReducersPushResultsBackToCoordinator()
        {
            var coordinatorStorage = new FileSystemStorage(@"c:\temp\netreduce\coordinator", true);

            var coordinatorProcess = Process.Start(new ProcessStartInfo(@"..\..\..\NetReduce.CoordinatorService.ConsoleHost\bin\Debug\NetReduce.CoordinatorService.ConsoleHost.exe"));
            Thread.Sleep(100);

            RemoteWorker<ServiceClientWrapper>.CoordinatorCallbackUri = new Uri("http://localhost:7777/CoordinatorService.svc");
            this.RemoteReduceWorks();

            Thread.Sleep(1000);

            var result = default(string);
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = coordinatorStorage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = coordinatorStorage.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (key == "k1")
                    {
                        result = coordinatorStorage.Read(fileName);
                    }
                }
            }

            result.ShouldBe("3");

            coordinatorProcess.Kill();
        }
    }
}
