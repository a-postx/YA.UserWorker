using System.Threading.Tasks;
using YA.UserWorker.Application.Models.Service;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IApiRequestDistributedCache
    {
        Task<bool> ApiRequestExist(string key);
        Task CreateApiRequestAsync(ApiRequest request);
        Task<ApiRequest> GetApiRequestAsync(string key);
        Task UpdateApiRequestAsync(ApiRequest request);
    }
}
