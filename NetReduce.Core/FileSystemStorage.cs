namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using NetReduce.Core.Exceptions;

    public class FileSystemStorage : IStorage
    {
        private string baseDirectory;

        public FileSystemStorage(string baseDirectory, bool eraseContents)
        {
            if (!Directory.Exists(baseDirectory))
            {
                try
                {
                    Directory.CreateDirectory(baseDirectory);
                }
                catch
                {
                    throw new DirectoryNotFoundException();
                }
            }

            this.baseDirectory = baseDirectory;

            if (eraseContents)
            {
                this.Clean();
            }
        }

        public IEnumerable<Uri> ListFiles()
        {
            return Directory.GetFiles(this.baseDirectory).Select(f => new Uri(string.Format("file:///{0}", Path.GetFileName(f))));
        }

        public string Read(string fileName)
        {
            var filePath = this.GetFilePathAndCheckIfExists(fileName);
            return File.ReadAllText(filePath);
        }

        public string Read(Uri uri)
        {
            return this.Read(this.GetFileName(uri));
        }

        public string[] ReadLines(string fileName)
        {
            var filePath = this.GetFilePathAndCheckIfExists(fileName);
            return File.ReadAllLines(filePath);
        }

        public void Store(string fileName, string value)
        {
            this.EnsureFileNameIsValid(fileName);
            var filePath = Path.Combine(this.baseDirectory, fileName);
            File.AppendAllText(filePath, value);
        }

        public void Store(Uri uri, string value)
        {
            this.Store(this.GetFileName(uri), value);
        }

        private string GetFilePathAndCheckIfExists(string fileName)
        {
            this.EnsureFileNameIsValid(fileName);

            var filePath = Path.Combine(this.baseDirectory, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            return filePath;
        }

        private void EnsureFileNameIsValid(string fileName)
        {
            if (fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            {
                throw new IllegalCharactersInFileNameException();
            }
        }

        public void Clean()
        {
            var files = Directory.GetFiles(this.baseDirectory);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public string GetFileName(Uri uri)
        {
            return uri.Segments.Last();
        }
    }
}
