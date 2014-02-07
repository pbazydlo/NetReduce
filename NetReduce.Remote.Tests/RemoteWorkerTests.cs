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
            var remoteServiceMock = new Mock<IRemoteWorkerService>(MockBehavior.Strict);
            var joinLock = new object();
            remoteServiceMock.Setup(s => s.TryJoin(It.IsAny<string>())).Returns(() =>
                {
                    lock(joinLock)
                    {
                        Monitor.Wait(joinLock);
                    }

                    return true;
                });

            var worker = new RemoteWorker()
            {
                RemoteWorkerService = remoteServiceMock.Object
            };

            var joinThread = new Thread(() =>
            {
                worker.Join();
            });

            joinThread.Start();
            Thread.Sleep(10);

            joinThread.ThreadState.ShouldBe(ThreadState.WaitSleepJoin);
            lock(joinLock)
            {
                Monitor.Pulse(joinLock);
            }

            joinThread.Join();
            remoteServiceMock.VerifyAll();
        }

        [TestMethod]
        public void RemoteWorkerIsAbleToPerformNonBlockingMap()
        {
            var uri = "ala";
            var mapFunc = "makota";
            var mapLock = new object();
            var remoteWorkerServiceMock = new Mock<IRemoteWorkerService>(MockBehavior.Strict);
            remoteWorkerServiceMock.Setup(s => s.Map(uri, mapFunc)).Callback(() =>
                {
                    lock(mapLock)
                    {
                        Monitor.Wait(mapLock);
                    }
                });

            RemoteWorker.NonBlockingMapAndReduce = true;
            var worker = new RemoteWorker()
            {
                RemoteWorkerService = remoteWorkerServiceMock.Object
            };

            worker.Map(uri, mapFunc);
            Thread.Sleep(10);
            lock(mapLock)
            {
                Monitor.Pulse(mapLock);
            }

            remoteWorkerServiceMock.VerifyAll();
        }
    }
}
