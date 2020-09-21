using System.Collections.Generic;

namespace YA.TenantWorker.Application.Models.BusinessModels
{
    public class PaginatedResultBm<T> : ValueObject where T : class
    {
        public PaginatedResultBm(bool hasNextPage, bool hasPreviousPage, int totalCount, List<T> items)
        {
            HasNextPage = hasNextPage;
            HasPreviousPage = hasPreviousPage;
            TotalCount = totalCount;
            Items = items;
        }

        public bool HasNextPage { get; private set; }
        public bool HasPreviousPage { get; private set; }
        public int TotalCount { get; private set; }
        public List<T> Items { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return HasNextPage;
            yield return HasPreviousPage;
            yield return TotalCount;
            yield return Items;
        }
    }
}
