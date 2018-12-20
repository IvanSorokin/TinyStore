using Newtonsoft.Json;

namespace TinyStore.Core
{
    internal class CachedObject
    {
        public string Json { get; set; }
        public object Object { get; set; }

        public T Value<T>()
        {
            if (Object != default)
                return (T)Object;
            return JsonConvert.DeserializeObject<T>(Json);
        }
    }
}
