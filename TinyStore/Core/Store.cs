using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TinyStore.Attributes;

namespace TinyStore.Core
{
    public class Store
    {
        private readonly TinyFs fs;
        private readonly bool keepDbInMemory;
        private readonly bool useTypeNameForCollection;

        public Store(string dbPath = null, bool useTypeNameForCollection = false, bool keepDbInMemory = false)
        {
            this.useTypeNameForCollection = useTypeNameForCollection;
            this.keepDbInMemory = keepDbInMemory;
            fs = new TinyFs(dbPath);
        }

        public void Save<T>(string id, T document, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            var json = JsonConvert.SerializeObject(document);
            fs.SaveToCollection(json, id, collectionName);
        }

        public T FindById<T>(string id, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            var json = fs.GetFromCollection(id, collectionName);
            return json != null ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }

        public IEnumerable<T> FindByQuery<T>(Func<T, bool> selector, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            return fs.GetCollection(collectionName)
                     .Select(x => JsonConvert.DeserializeObject<T>(x))
                     .Where(selector);
        }

        public void DeleteById<T>(string id, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            fs.Delete(id, collectionName);
        }

        public void DeleteByQuery<T>(Func<T, bool> selector, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            var paths = fs.GetCollectionFiles(collectionName)
                          .Select(x => (path: x, obj: JsonConvert.DeserializeObject<T>(fs.GetFile(x))))
                          .Where(x => selector(x.obj))
                          .Select(x => x.path);
            foreach (var path in paths)
                fs.DeleteFile(path);
        }


        private string GetCollectionName(Type type, string collectionName)
        {
            var name = collectionName ??
                       (useTypeNameForCollection ? type.Name : null) ??
                       (Attribute.GetCustomAttribute(type,
                                                     typeof(CollectionNameAttribute)) as CollectionNameAttribute)?.Name;
            if (name == null)
                throw new ArgumentException("No collection name was provided and type had " +
                	"no CollectionNameAttribute and UseTypeNameForCollection was false");

            return name;
        }
    }
}
