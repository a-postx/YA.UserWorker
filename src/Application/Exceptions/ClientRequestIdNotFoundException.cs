using System;

namespace YA.TenantWorker.Application.Exceptions
{
    public class ClientRequestIdNotFoundException : Exception
    {
        public ClientRequestIdNotFoundException()
        {
        }

        public ClientRequestIdNotFoundException(string message) : base(message)
        {
        }

        public ClientRequestIdNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
