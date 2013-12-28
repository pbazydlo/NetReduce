using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Core
{
    public class ThreadWorker : IWorker
    {
        private IStorage storage;
        private Thread workerThread;

        public ThreadWorker(IStorage storage)
        {
            this.storage = storage;
        }

        public void Map(string inputFileName, string mapCodeFileName)
        {
            var mapProvider = Loader.Load<IMapProvider>(mapCodeFileName, this.storage);
            var mapper = new Mapper(inputFileName, mapProvider.Map, this.storage);
            var mapResult = mapper.PerformMap();
        }

        public void Reduce(string key, string reduceCodeFileName)
        {
            throw new NotImplementedException();
        }

        public void Join()
        {
            throw new NotImplementedException();
        }
    }
}
