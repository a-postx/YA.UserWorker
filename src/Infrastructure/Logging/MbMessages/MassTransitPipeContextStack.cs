using GreenPipes;
using System;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    /// <summary>
    /// Manages a Logical Call Context variable containing a stack of <see cref="PipeContext"/> instances.
    /// </summary>
    static class MassTransitPipeContextStack
    {
        /// <summary>
        /// Publishes a <see cref="PipeContext"/> onto the stack.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDisposable Push(PipeContext context)
        {
            return AsyncLocalStack<PipeContext>.Push(context);
        }

        /// <summary>
        /// Gets the current <see cref="PipeContext"/>.
        /// </summary>
        public static PipeContext Current => AsyncLocalStack<PipeContext>.Current;
    }
}
