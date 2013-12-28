using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetReduce.Core.Tests
{
    internal static class TestHelpers
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
