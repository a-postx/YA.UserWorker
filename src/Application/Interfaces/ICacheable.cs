using System;

namespace YA.UserWorker.Application.Interfaces
{
    public interface ICacheable
    {
        public string CacheKey { get; }
        public TimeSpan AbsoluteExpiration { get; }
    }
}
