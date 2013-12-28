using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetReduce.Core.Tests
{
    internal static class TestHelpers
    {
        public static void ClearAndCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }

            Directory.CreateDirectory(path);
        }
    }
}
