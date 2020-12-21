using System.Collections.Generic;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IPaginatedResultFactory
    {
        PaginatedResultVm<T> GetPaginatedResult<T>(PageOptions pageOptions, bool hasNextPage, bool hasPreviousPage, int totalCount, string startCursor, string endCursor, string routeName, List<T> itemVms) where T : class;
    }
}
