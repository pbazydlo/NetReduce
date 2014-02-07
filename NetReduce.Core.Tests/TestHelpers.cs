namespace NetReduce.Core.Tests
{
    using System.IO;

    public static class TestHelpers
    {
        public static void LoadToStorage(string realPath, string storageFileName, IStorage storage)
        {
            using (var sr = new StreamReader(realPath))
            {
                storage.Store(storageFileName, sr.ReadToEnd());
            }
        }
    }
}
