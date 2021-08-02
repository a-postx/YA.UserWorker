using System;

namespace YA.UserWorker.Application.Exceptions
{
    public class IdempotencyKeyNotFoundException : Exception
    {
        public IdempotencyKeyNotFoundException()
        {
        }

        public IdempotencyKeyNotFoundException(string message) : base(message)
        {
        }

        public IdempotencyKeyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
