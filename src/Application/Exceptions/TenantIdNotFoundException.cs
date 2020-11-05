using System;

namespace YA.TenantWorker.Application.Exceptions
{
    public class TenantIdNotFoundException : Exception
    {
        public TenantIdNotFoundException()
        {
        }

        public TenantIdNotFoundException(string message) : base(message)
        {
        }

        public TenantIdNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
