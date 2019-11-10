using System.Collections.Generic;

namespace YA.TenantWorker.Application.Models.ViewModels
{
    public class Connection<T>
    {
        public int TotalCount { get; set; }

        public PageInfo PageInfo { get; set; }

        public List<T> Items { get; set; }
    }
}
