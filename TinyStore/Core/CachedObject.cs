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
            var deserialized = JsonConvert.DeserializeObject<T>(Json);
            Object = deserialized;
            return deserialized;
        }
    }
}