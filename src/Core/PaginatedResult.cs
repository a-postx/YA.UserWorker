using System.Collections.Generic;

namespace YA.TenantWorker.Core
{
    public class PaginatedResult<T> where T : class
    {
        public PaginatedResult(int totalCount, ICollection<T> items)
        {
            TotalCount = totalCount;
            Items = items;
        }

        public int TotalCount { get; private set; }
        public ICollection<T> Items { get; private set; }
    }
}
