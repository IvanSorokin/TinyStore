using System.Collections.Generic;
using System.Linq;

namespace TinyStore.Core
{
    internal class CachedStore
    {
        private readonly Dictionary<string, Dictionary<string, object>> collections = new Dictionary<string, Dictionary<string, object>>();

        public void Save(string collectionName, string id, object obj)
        {
            if (collections.ContainsKey(collectionName))
                collections[collectionName][id] = obj;
            else
            {
                collections[collectionName] = new Dictionary<string, object>();
                collections[collectionName][id] = obj;
            }
        }

        public IEnumerable<object> Get(string collectionName, IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (collections.ContainsKey(collectionName) && collections[collectionName].ContainsKey(id))
                    yield return collections[collectionName][id];
            }
        }

        public void Delete(string collectionName, IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (collections.ContainsKey(collectionName) && collections[collectionName].ContainsKey(id))
                    collections[collectionName].Remove(id);
            }
        }

        public IEnumerable<object> GetCollection(string collectionName)
        {
            if (collections.ContainsKey(collectionName))
                return collections[collectionName].Select(x => x.Value);
            return Enumerable.Empty<object>();
        }
    }
}
