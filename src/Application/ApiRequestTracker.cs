﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application
{
    /// <summary>
    /// Отслеживает АПИ-запросы. На машине разработчика даёт +50мс к запросу, поэтому нужно подумать о смене хранилища
    /// на что-то с хорошей скоростью записи (Редис, Монго и т.п.) или оставить кеширование в памяти
    /// </summary>
    public class ApiRequestTracker : IApiRequestTracker
    {
        public ApiRequestTracker(ILogger<ApiRequestTracker> logger, IApiRequestMemoryCache apiRequestCache, ITenantWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiRequestCache = apiRequestCache ?? throw new ArgumentNullException(nameof(apiRequestCache));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<ApiRequestTracker> _log;
        private readonly IApiRequestMemoryCache _apiRequestCache;
        private readonly ITenantWorkerDbContext _dbContext;
        
        public async Task<(bool created, ApiRequest request)> GetOrCreateRequestAsync(Guid correlationId, string method, CancellationToken cancellationToken)
        {
            (bool requestFoundInCache, ApiRequest request) = await GetFromCacheOrDbAsync(correlationId, cancellationToken);

            if (requestFoundInCache)
            {
                return (false, request);
            }
            else
            {
                if (request != null)
                {
                    _apiRequestCache.Add(request, request.ApiRequestID);
                    return (false, request);
                }
                else
                {
                    ApiRequest newApiRequest = new ApiRequest(correlationId, DateTime.UtcNow, method);

                    ApiRequest createdRequest = await _dbContext.CreateApiRequestAsync(newApiRequest, cancellationToken);
                    await _dbContext.ApplyChangesAsync(cancellationToken);

                    _apiRequestCache.Add(newApiRequest, newApiRequest.ApiRequestID);

                    return (true, createdRequest);
                }
            }
        }

        private async Task<(bool requestFoundInCache, ApiRequest request)> GetFromCacheOrDbAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            ApiRequest requestFromCache = _apiRequestCache.GetApiRequestFromCache<ApiRequest>(correlationId);
                
            if (requestFromCache == null)
            {
                ApiRequest request = await _dbContext.GetApiRequestAsync(e => e.ApiRequestID == correlationId, cancellationToken);

                return (request != null) ? (false, request) : (false, null);
            }
            else
            {
                return (true, requestFromCache);
            }
        }

        public async Task SetResultAsync(ApiRequest request, ApiRequestResult result, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new Exception("Api request cannot be empty.");
            }

            if (result == null)
            {
                throw new Exception("Api request result cannot be empty.");
            }

            request.SetResponseStatusCode(result.StatusCode);
            request.SetResponseBody((result.Body != null) ? JToken.Parse(JsonConvert.SerializeObject(result.Body)).ToString(Formatting.Indented) : null);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            _apiRequestCache.Update(request, request.ApiRequestID);
        }
    }
}
