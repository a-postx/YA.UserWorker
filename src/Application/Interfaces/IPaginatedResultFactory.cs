using System.Collections.Generic;
using YA.UserWorker.Application.Models.HttpQueryParams;
using YA.UserWorker.Application.Models.ViewModels;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IPaginatedResultFactory
    {
        PaginatedResultVm<T> GetCursorPaginatedResult<T>(PageOptionsCursor pageOptions, bool hasNextPage, bool hasPreviousPage, int totalCount, string startCursor, string endCursor, string routeName, ICollection<T> items) where T : class;
        PaginatedResultVm<T> GetOffsetPaginatedResult<T>(PageOptionsOffset pageOptions, int totalCount, string routeName, ICollection<T> items) where T : class;
    }
}
