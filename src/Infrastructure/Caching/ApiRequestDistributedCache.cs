using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Service;

namespace YA.TenantWorker.Infrastructure.Caching
{
    /// <summary>
    /// Распределённый кеш АПИ-запросов, реализованный с помощью Редис
    /// </summary>
    public class ApiRequestDistributedCache : IApiRequestDistributedCache
    {
        public ApiRequestDistributedCache(ILogger<ApiRequestDistributedCache> logger,
            IRuntimeContextAccessor runtimeContext,
            ConnectionMultiplexer connectionMultiplexer)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
            _cacheDb = connectionMultiplexer.GetDatabase();
        }

        private readonly ILogger<ApiRequestDistributedCache> _log;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IDatabase _cacheDb;

        private const string Id = "id";
        private const string Method = "method";
        private const string Path = "path";
        private const string Query = "query";
        private const string ResultStatus = "status";
        private const string ResultHeaders = "headers";
        private const string ResultBody = "body";
        private const string ResultRouteName = "routename";
        private const string ResultRouteValues = "routevalues";

        public async Task<bool> ApiRequestExist(string key)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool hashKeyExists = false;

            try
            {
                hashKeyExists = await _cacheDb.KeyExistsAsync(key);
            }
            catch (RedisConnectionException ex)
            {
                _log.LogError(ex, "Connection error getting cached value for key {CacheKey}", key);
            }
            //в случае перезагрузки Редиса (установка патчей на Азуре)
            catch (RedisTimeoutException ex)
            {
                _log.LogError(ex, "Timeout error getting cached value for key {CacheKey}", key);
            }
            catch (RedisException ex)
            {
                _log.LogError(ex, "Error getting cached value for key {CacheKey}", key);
            }

            sw.Stop();
            _log.LogInformation("cache.request.idempotency.exist.msec {CacheRequestIdempotencyExistMsec}", sw.ElapsedMilliseconds);

            return hashKeyExists;
        }

        public async Task<ApiRequest> GetApiRequestAsync(string key)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            HashEntry[] hashEntries = null;

            try
            {
                hashEntries = await _cacheDb.HashGetAllAsync(key);
            }
            catch (RedisConnectionException ex)
            {
                _log.LogError(ex, "Connection error getting cached value for key {CacheKey}", key);
            }
            catch (RedisTimeoutException ex)
            {
                _log.LogError(ex, "Timeout error getting cached value for key {CacheKey}", key);
            }
            catch (RedisException ex)
            {
                _log.LogError(ex, "Error getting cached value for key {CacheKey}", key);
            }

            sw.Stop();
            _log.LogInformation("cache.request.idempotency.get.msec {CacheRequestIdempotencyGetMsec}", sw.ElapsedMilliseconds);

            if (hashEntries != null && hashEntries.Length > 0)
            {
                ApiRequest cachedRequest = new ApiRequest(_runtimeCtx.GetTenantId(), _runtimeCtx.GetClientRequestId());

                RedisValue methodItem = hashEntries
                    .Where(i => i.Name == Method).Select(i => i.Value).FirstOrDefault();

                if (!methodItem.IsNullOrEmpty)
                {
                    cachedRequest.SetMethod(methodItem);
                }

                RedisValue pathItem = hashEntries
                    .Where(i => i.Name == Path).Select(i => i.Value).FirstOrDefault();

                if (!pathItem.IsNullOrEmpty)
                {
                    cachedRequest.SetPath(pathItem);
                }

                RedisValue queryItem = hashEntries
                    .Where(i => i.Name == Query).Select(i => i.Value).FirstOrDefault();

                if (!queryItem.IsNullOrEmpty)
                {
                    cachedRequest.SetQuery(queryItem);
                }

                RedisValue statusItem = hashEntries
                    .Where(i => i.Name == ResultStatus).Select(i => i.Value).FirstOrDefault();

                if (!statusItem.IsNullOrEmpty)
                {
                    int statusCode = statusItem.HasValue ? Convert.ToInt32(statusItem, CultureInfo.InvariantCulture) : 0;
                    cachedRequest.SetStatusCode(statusCode);
                }

                RedisValue bodyItem = hashEntries
                    .Where(i => i.Name == ResultBody).Select(i => i.Value).FirstOrDefault();

                if (!bodyItem.IsNullOrEmpty)
                {
                    cachedRequest.SetBody(bodyItem);
                }

                RedisValue headersItem = hashEntries
                    .Where(i => i.Name == ResultHeaders).Select(i => i.Value).FirstOrDefault();

                if (!headersItem.IsNullOrEmpty)
                {
                    Dictionary<string, List<string>> headers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(headersItem);
                    cachedRequest.SetHeaders(headers);
                }

                RedisValue routeNameItem = hashEntries
                    .Where(i => i.Name == ResultRouteName).Select(i => i.Value).FirstOrDefault();

                if (!routeNameItem.IsNullOrEmpty)
                {
                    cachedRequest.SetResultRouteName(routeNameItem);
                }

                RedisValue routeValuesItem = hashEntries
                    .Where(i => i.Name == ResultRouteValues).Select(i => i.Value).FirstOrDefault();

                if (!routeValuesItem.IsNullOrEmpty)
                {
                    Dictionary<string, string> routeValues = JsonSerializer.Deserialize<Dictionary<string, string>>(routeValuesItem);
                    cachedRequest.SetResultRouteValues(routeValues);
                }

                return cachedRequest;
            }
            else
            {
                return null;
            }
        }

        public async Task CreateApiRequestAsync(ApiRequest request)
        {
            List<HashEntry> hashList = new List<HashEntry>
            {
                new HashEntry(Id, request.ApiRequestID.ToString()),
                new HashEntry(Method, request.Method)
            };

            if (!string.IsNullOrEmpty(request.Path))
            {
                hashList.Add(new HashEntry(Path, request.Path));
            }

            if (!string.IsNullOrEmpty(request.Query))
            {
                hashList.Add(new HashEntry(Query, request.Query));
            }

            HashEntry[] apiRequestHashSet = hashList.ToArray();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await _cacheDb.HashSetAsync(request.CacheKey, apiRequestHashSet);
                await _cacheDb.KeyExpireAsync(request.CacheKey, request.AbsoluteExpiration);
            }
            catch (RedisConnectionException ex)
            {
                _log.LogError(ex, "Connection error on adding value for key {CacheKey}", request.CacheKey);
            }
            catch (RedisTimeoutException ex)
            {
                _log.LogError(ex, "Timeout error on adding value for key {CacheKey}", request.CacheKey);
            }
            catch (RedisException ex)
            {
                _log.LogError(ex, "Error adding cached value for key {CacheKey}", request.CacheKey);
            }

            sw.Stop();
            _log.LogInformation("cache.request.idempotency.create.msec {CacheRequestIdempotencyCreateMsec}", sw.ElapsedMilliseconds);
        }

        public async Task UpdateApiRequestAsync(ApiRequest request)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<HashEntry> hashList = new List<HashEntry>
            {
                new HashEntry(ResultStatus, request.StatusCode ?? 0)
            };

            string serializedHeaders = JsonSerializer.Serialize(request.Headers);

            if (!string.IsNullOrEmpty(serializedHeaders))
            {
                hashList.Add(new HashEntry(ResultHeaders, serializedHeaders));
            }

            if (!string.IsNullOrEmpty(request.Body))
            {
                hashList.Add(new HashEntry(ResultBody, request.Body));
            }

            if (!string.IsNullOrEmpty(request.ResultRouteName))
            {
                hashList.Add(new HashEntry(ResultRouteName, request.ResultRouteName));
            }

            if (request.ResultRouteValues?.Count > 0)
            {
                string serializedRouteValues = JsonSerializer.Serialize(request.ResultRouteValues);

                if (!string.IsNullOrEmpty(serializedRouteValues) && serializedRouteValues != "null")
                {
                    hashList.Add(new HashEntry(ResultRouteValues, serializedRouteValues));
                }
            }

            HashEntry[] redisApiReqHashSet = hashList.ToArray();

            try
            {
                await _cacheDb.HashSetAsync(request.CacheKey, redisApiReqHashSet);
            }
            catch (RedisConnectionException ex)
            {
                _log.LogError(ex, "Connection error on updating value for key {CacheKey}", request.CacheKey);
            }
            catch (RedisTimeoutException ex)
            {
                _log.LogError(ex, "Timeout error updating cached value for key {CacheKey}", request.CacheKey);
            }
            catch (RedisException ex)
            {
                _log.LogError(ex, "Error updating cached value for key {CacheKey}", request.CacheKey);
            }

            sw.Stop();
            _log.LogInformation("cache.request.idempotency.update.msec {CacheRequestIdempotencyUpdateMsec}", sw.ElapsedMilliseconds);
        }
    }
}
