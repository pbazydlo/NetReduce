namespace NetReduce.Core
{
    using System;

    public interface IWorker
    {
        void Map(Uri inputFileName, Uri mapCodeFileName);
        void Reduce(string key, Uri reduceCodeFileName);
        void Join();
        IStorage Storage { get; set; }
        int Id { get; set; }
    }
}
