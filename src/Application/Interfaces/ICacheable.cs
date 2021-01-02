using System;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface ICacheable
    {
        public string CacheKey { get; }
        public TimeSpan AbsoluteExpiration { get; }
    }
}
