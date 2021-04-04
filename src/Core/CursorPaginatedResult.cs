using System.Collections.Generic;

namespace YA.UserWorker.Core
{
    public class CursorPaginatedResult<T> : PaginatedResult<T> where T : class
    {
        public CursorPaginatedResult(bool hasNextPage, bool hasPreviousPage, int totalCount, ICollection<T> items) : base(totalCount, items)
        {
            HasNextPage = hasNextPage;
            HasPreviousPage = hasPreviousPage;
        }

        public bool HasNextPage { get; private set; }
        public bool HasPreviousPage { get; private set; }
    }
}
