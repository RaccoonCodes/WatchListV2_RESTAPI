using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace WatchListV2.Extensions
{
    //Distributed cache
    public static class DistributedCacheExtensions
    {
        //Attemps to retrieve cache and deserialize it 
        public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
        {
            value = default;
            var val = cache.Get(key);

            if (val == null)
            {
                return false;
            }
            value = JsonSerializer.Deserialize<T>(val);
            return true;

        }

        //Store a value in cache after serializing it to a JSON
        public static void Set<T>(this IDistributedCache cache, string key, T value, TimeSpan timeSpan)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
            cache.Set(key, bytes, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = timeSpan
            });
        }
    }
}
