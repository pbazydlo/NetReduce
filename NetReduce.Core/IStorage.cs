namespace NetReduce.Core
{
    using System.Collections.Generic;

    public interface IStorage
    {
        IEnumerable<string> ListFiles();
        string Read(string fileName);
        string[] ReadLines(string fileName);
        void Store(string fileName, string value);
        void Clean();
    }
}
