using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Shouldly;

namespace NetReduce.Remote.Tests
{
    [TestClass]
    public class RemoteWorkerTests
    {
        [TestMethod]
        public void RemoteWorkerWaitsForJoinInRemoteNode()
        {
            var joinLock = new object();
            var uriProviderMock = GetSimpleUriProvider();
            var remoteServiceMock = new Mock<IRemoteWorkerService>(MockBehavior.Strict);
            remoteServiceMock.Setup(s => s.Init(It.IsAny<int>()));
            remoteServiceMock.Setup(s => s.TryJoin(It.IsAny<int>(), It.IsAny<Uri>())).Returns(() =>
                {
                    lock (joinLock)
                    {
                        Monitor.Wait(joinLock);
                    }

                    return new string[] { "a" };
                });

            MockerForRemoteWorkerService.Mock = remoteServiceMock.Object;
            RemoteWorker<MockerForRemoteWorkerService>.UriProvider = uriProviderMock.Object;
            var worker = new RemoteWorker<MockerForRemoteWorkerService>();

            var joinThread = new Thread(() =>
            {
                worker.Join();
            });

            joinThread.Start();
            Thread.Sleep(10);

            joinThread.ThreadState.ShouldBe(ThreadState.WaitSleepJoin);
            lock (joinLock)
            {
                Monitor.Pulse(joinLock);
            }

            joinThread.Join();
            remoteServiceMock.VerifyAll();
        }

        [TestMethod]
        public void RemoteWorkerIsAbleToPerformNonBlockingMap()
        {
            var uri = new Uri("file:///ala");
            var mapFunc = new Uri("file:///makota");
            var mapLock = new object();
            var uriProviderMock = GetSimpleUriProvider();
            var remoteWorkerServiceMock = new Mock<IRemoteWorkerService>(MockBehavior.Strict);
            remoteWorkerServiceMock.Setup(s => s.Init(It.IsAny<int>()));
            remoteWorkerServiceMock.Setup(s => s.Map(uri, mapFunc)).Callback(() =>
                {
                    lock (mapLock)
                    {
                        Monitor.Wait(mapLock);
                    }
                });

            MockerForRemoteWorkerService.Mock = remoteWorkerServiceMock.Object;
            RemoteWorker<MockerForRemoteWorkerService>.UriProvider = uriProviderMock.Object;
            RemoteWorker<MockerForRemoteWorkerService>.NonBlockingMapAndReduce = true;
            var worker = new RemoteWorker<MockerForRemoteWorkerService>();

            worker.Map(uri, mapFunc);
            Thread.Sleep(10);
            lock (mapLock)
            {
                Monitor.Pulse(mapLock);
            }

            remoteWorkerServiceMock.VerifyAll();
        }

        private static Mock<IUriProvider> GetSimpleUriProvider()
        {
            var uriProviderMock = new Mock<IUriProvider>(MockBehavior.Strict);
            uriProviderMock.Setup(up => up.GetNextUri()).Returns(new Uri("http://localhost:12345/ser.svc"));
            return uriProviderMock;
        }
    }
}
