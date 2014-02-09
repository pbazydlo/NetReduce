namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;

    public interface IStorage
    {
        IEnumerable<Uri> ListFiles();
        string Read(string fileName);
        string Read(Uri uri);
        string[] ReadLines(string fileName);
        void Store(string fileName, string value);
        void Store(Uri uri, string value);
        void Clean();
        string GetFileName(Uri uri);
        IEnumerable<string> GetKeys();
        void Remove(Uri uri);
    }
}
