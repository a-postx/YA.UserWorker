using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Service;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IApiRequestDistributedCache
    {
        Task<bool> ApiRequestExist(string key);
        Task CreateApiRequestAsync(ApiRequest request);
        Task<ApiRequest> GetApiRequestAsync(string key);
        Task UpdateApiRequestAsync(ApiRequest request);
    }
}
