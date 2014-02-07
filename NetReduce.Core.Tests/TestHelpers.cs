namespace NetReduce.Core.Tests
{
    using System;
    using System.IO;

    public static class TestHelpers
    {
        public static void LoadToStorage(string realPath, Uri storageFileName, IStorage storage)
        {
            using (var sr = new StreamReader(realPath))
            {
                storage.Store(storage.GetFileName(storageFileName), sr.ReadToEnd());
            }
        }
    }
}
