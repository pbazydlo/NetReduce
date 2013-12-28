namespace NetReduce.Core
{
    public interface IWorker
    {
        void Map(string inputFileName, string mapCodeFileName);
        void Reduce(string key, string reduceCodeFileName);
        void Join();
        IStorage Storage { get; set; }
        int Id { get; set; }
    }
}
