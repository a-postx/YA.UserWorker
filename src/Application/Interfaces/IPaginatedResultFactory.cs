using System.Collections.Generic;
using YA.TenantWorker.Application.Models.HttpQueryParams;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IPaginatedResultFactory
    {
        PaginatedResultVm<T> GetPaginatedResult<T>(PageOptions pageOptions, bool hasNextPage, bool hasPreviousPage, int totalCount, string startCursor, string endCursor, string routeName, ICollection<T> items) where T : class;
    }
}
