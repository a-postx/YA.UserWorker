using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IApiRequestManager
    {
        Task<(bool created, ApiRequest request)> GetOrCreateRequestAsync(Guid correlationId, string method, CancellationToken cancellationToken);
        Task SetResultAsync(ApiRequest request, ApiRequestResult requestResult, CancellationToken cancellationToken);
    }
}