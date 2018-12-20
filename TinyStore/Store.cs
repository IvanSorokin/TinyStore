using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TinyStore.Attributes;
using TinyStore.Core;

namespace TinyStore
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
                cachedStore = new CachedStore(fs);
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
                return cachedStore.Get<T>(collectionName, new[] { id }).Cast<T>().SingleOrDefault();

            var json = fs.GetFromCollection(id, collectionName);
            return json != null ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }

        public IEnumerable<T> FindByQuery<T>(Func<T, bool> filter, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            if (cachedStore != null)
                return cachedStore.GetCollection<T>(collectionName).Cast<T>().Where(filter);

            return fs.GetCollection(collectionName)
                     .Select(x => JsonConvert.DeserializeObject<T>(x))
                     .Where(filter);
        }

        public void DeleteById<T>(string id, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            fs.Delete(id, collectionName);
            cachedStore?.Delete(collectionName, new[] { id });
        }

        public void DeleteByQuery<T>(Func<T, bool> filter, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            foreach (var pair in GetFilteredPairs(filter, collectionName))
            {
                fs.Delete(pair.id, collectionName);
                cachedStore?.Delete(collectionName, new[] { pair.id });
            }
        }

        public void Modify<T>(string id, Action<T> modify, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);
            var doc = FindById<T>(id, collectionName);
            if (doc != default)
            {
                modify(doc);
                Save(id, doc, collectionName);
            }
        }

        public void ModifyByQuery<T>(Func<T, bool> filter, Action<T> modify, string collectionName = null)
        {
            collectionName = GetCollectionName(typeof(T), collectionName);

            foreach (var pair in GetFilteredPairs(filter, collectionName))
            {
                modify(pair.obj);
                Save(pair.id, pair.obj, collectionName);
            }
        }

        private IEnumerable<(string id, T obj)> GetFilteredPairs<T>(Func<T, bool> filter, string collectionName)
        {
            return fs.GetCollectionFiles(collectionName)
                     .Select(x => (id: x, obj: JsonConvert.DeserializeObject<T>(fs.GetFromCollection(x, collectionName))))
                     .Where(x => filter(x.obj));
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
