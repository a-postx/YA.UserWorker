using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Infrastructure.Caching
{
    public class ApiRequestMemoryCache : YaMemoryCache, IApiRequestMemoryCache
    {
        public ApiRequestMemoryCache()
        {
            MemoryCacheOptions cacheOptions = new MemoryCacheOptions { SizeLimit = 256 };
            SetOptions(cacheOptions);
        }

        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(120))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

        public async Task<(bool created, T request)> GetOrCreateAsync<T>(Guid key, Func<Task<T>> createFunc) where T : class
        {
            (bool created, T request) result = await base.GetOrCreateAsync(key, createFunc, _cacheOptions);
            return result;
        }

        public void Add<T>(T request, Guid key) where T : class
        {
            base.Set(key, request, _cacheOptions);
        }

        public T GetApiRequestFromCache<T>(Guid key) where T : class
        {
            return base.Get<T>(key);
        }

        public void Update<T>(T request, Guid key) where T : class
        {
            base.Update(key, request, _cacheOptions);
        }
    }
}
