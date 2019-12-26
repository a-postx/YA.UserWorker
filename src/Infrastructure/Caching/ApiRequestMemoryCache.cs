using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Caching
{
    public class ApiRequestMemoryCache : YaMemoryCache<ApiRequest>
    {
        public ApiRequestMemoryCache()
        {
            MemoryCacheOptions options = new MemoryCacheOptions { SizeLimit = General.ApiRequestsCacheSize };
            SetOptions(options);
        }

        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(General.ApiRequestCacheSlidingExpirationSec))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(General.ApiRequestCacheAbsoluteExpirationSec));            

        public async Task<(bool created, ApiRequest request)> GetOrCreateAsync(object key, Func<Task<ApiRequest>> createItem)
        {
            (bool created, ApiRequest request) result = await base.GetOrCreateAsync(key, createItem, _cacheOptions);
            return result;
        }

        public void Add(ApiRequest request)
        {
            base.Set(request.ApiRequestID, request, _cacheOptions);
        }
        
        public ApiRequest GetApiRequestFromCache(object key)
        {
            return base.Get(key);
        }

        internal void Update(ApiRequest request)
        {
            base.Update(request.ApiRequestID, request, _cacheOptions);
        }
    }
}
