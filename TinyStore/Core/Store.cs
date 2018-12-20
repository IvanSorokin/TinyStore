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
        private readonly CachedStore cachedStore;

        public Store(string dbPath = null, bool useTypeNameForCollection = false, bool keepDbInMemory = false)
        {
            this.useTypeNameForCollection = useTypeNameForCollection;
            this.keepDbInMemory = keepDbInMemory;
            if (keepDbInMemory)
                cachedStore = new CachedStore();
            fs = new TinyFs(dbPath);
        }

        public void Save<T>(string id, T document, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            var json = JsonConvert.SerializeObject(document);
            fs.SaveToCollection(json, id, collectionName);
            cachedStore?.Save(collectionName, id, document);
        }

        public T FindById<T>(string id, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            if (cachedStore != null)
                return cachedStore.Get(collectionName, new[] { id }).Cast<T>().SingleOrDefault();

            var json = fs.GetFromCollection(id, collectionName);
            return json != null ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }

        public IEnumerable<T> FindByQuery<T>(Func<T, bool> selector, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            if (cachedStore != null)
                return cachedStore.GetCollection(collectionName).Cast<T>().Where(selector);

            return fs.GetCollection(collectionName)
                     .Select(x => JsonConvert.DeserializeObject<T>(x))
                     .Where(selector);
        }

        public void DeleteById<T>(string id, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            fs.Delete(id, collectionName);
            cachedStore?.Delete(collectionName, new[] { id });
        }

        public void DeleteByQuery<T>(Func<T, bool> selector, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            var pairs = fs.GetCollectionFiles(collectionName)
                          .Select(x => (id: x, obj: JsonConvert.DeserializeObject<T>(fs.GetFromCollection(x, collectionName))))
                          .Where(x => selector(x.obj));

            foreach (var pair in pairs)
            {
                fs.Delete(pair.id, collectionName);
                cachedStore?.Delete(collectionName, new[] { pair.id });
            }
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
