using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TinyStore.Core
{
    internal class TinyFs
    {
        private readonly string rootDirectory;

        public TinyFs(string dbPath)
        {
            rootDirectory = dbPath != null ? Path.Combine(dbPath, "tinyDb") : "tinyDb";
            if (!Directory.Exists(rootDirectory))
                Directory.CreateDirectory(rootDirectory);
        }

        private void EnsureCollectionFolder(string name)
        {
            var path = Path.Combine(rootDirectory, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public bool SaveToCollection(string json, string id, string collectionName)
        {
            EnsureCollectionFolder(collectionName);
            File.WriteAllText(Path.Combine(rootDirectory, collectionName, id), json);
            return true;
        }

        public string GetFromCollection(string id, string collectionName)
        {
            var path = Path.Combine(rootDirectory, collectionName, id);
            if (File.Exists(path))
                return File.ReadAllText(path);
            return null;
        }

        public void Delete(string id, string collectionName)
        {
            var path = Path.Combine(rootDirectory, collectionName, id);
            if (File.Exists(path))
                File.Delete(path);
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public string GetFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
            return null;
        }

        public IEnumerable<string> GetCollectionFiles(string collectionName)
        {
            return Directory.EnumerateFiles(Path.Combine(rootDirectory, collectionName));
        }

        public IEnumerable<string> GetCollection(string collectionName)
        {
            return Directory.EnumerateFiles(Path.Combine(rootDirectory, collectionName))
                            .Select(x => File.ReadAllText(x));
        }
    }
}
