namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using NetReduce.Core.Exceptions;
    using System.Text.RegularExpressions;
    using NetReduce.Core.Extensions;

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
            File.WriteAllText(filePath, value);
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

            if (this.baseDirectory.StartsWith(@"c:\temp"))
            {
                var directories = Directory.GetDirectories(this.baseDirectory);
                foreach (var directory in directories)
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        public string GetFileName(Uri uri)
        {
            return uri.Segments.Last();
        }


        public IEnumerable<string> GetKeys()
        {
            var result = new List<string>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = this.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = this.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (!result.Contains(key))
                    {
                        result.Add(key);
                    }
                }
            }

            return result;
        }
    }
}
