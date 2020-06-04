﻿using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace YA.TenantWorker.Infrastructure.Caching
{
    public class YaMemoryCache : IDisposable
    {
        private MemoryCache _cache;
        private bool disposedValue;

        protected void SetOptions(MemoryCacheOptions options)
        {
            _cache = new MemoryCache(options);
        }

        public async Task<(bool created, T type)> GetOrCreateAsync<T>(object key, Func<Task<T>> createItem, MemoryCacheEntryOptions options) where T : class
        {
            if (createItem == null)
            {
                throw new ArgumentNullException(nameof(createItem));
            }

            bool itemExists = _cache.TryGetValue(key, out T cacheEntry);

            if (!itemExists)
            {
                T newItem = await createItem();

                MemoryCacheEntryOptions cacheEntryOptions = options;

                _cache.Set(key, newItem, cacheEntryOptions);

                return (true, newItem);
            }
            else
            {
                return (false, cacheEntry);
            }
        }

        public T Set<T>(object key, T cacheEntry, MemoryCacheEntryOptions options) where T : class
        {
            return _cache.Set(key, cacheEntry, options) as T;
        }

        public T Get<T>(object key) where T : class
        {
            return (T)_cache.Get(key);
        }

        public T Update<T>(object key, T newCacheEntry, MemoryCacheEntryOptions options) where T : class
        {
            _cache.Remove(key);
            return _cache.Set(key, newCacheEntry, options);
        }

        public void Remove<T>(object key) where T : class
        {
            _cache.Remove(key);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }

                _cache = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
