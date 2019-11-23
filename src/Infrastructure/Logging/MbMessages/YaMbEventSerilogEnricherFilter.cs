using System.Threading.Tasks;
using GreenPipes;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    /// <summary>
    /// Maintains the current pipe context on the logical call stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class YaMbEventSerilogEnricherFilter<T> : IFilter<T> where T : class, PipeContext
    {
        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("YaMbEventSerilogEnricher");
        }

        public async Task Send(T context, IPipe<T> next)
        {
            using (MassTransitPipeContextStack.Push(context))
            {
                await next.Send(context);
            }
        }
    }
}
