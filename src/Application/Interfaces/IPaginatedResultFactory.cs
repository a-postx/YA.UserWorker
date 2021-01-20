using System.Collections.Generic;
using YA.TenantWorker.Application.Models.HttpQueryParams;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IPaginatedResultFactory
    {
        PaginatedResultVm<T> GetCursorPaginatedResult<T>(PageOptionsCursor pageOptions, bool hasNextPage, bool hasPreviousPage, int totalCount, string startCursor, string endCursor, string routeName, ICollection<T> items) where T : class;
        PaginatedResultVm<T> GetOffsetPaginatedResult<T>(PageOptionsOffset pageOptions, int totalCount, string routeName, ICollection<T> items) where T : class;
    }
}
