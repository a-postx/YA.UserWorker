using System;
using System.Threading.Tasks;
using MassTransit;

namespace YA.TenantWorker.MessageBus
{
    public class TestRequestConsumer : IConsumer<ITenantWorkerTestRequestV1>
    {
        public TestRequestConsumer()
        {

        }

        public async Task Consume(ConsumeContext<ITenantWorkerTestRequestV1> context)
        {
            await context.RespondAsync<ITenantWorkerTestResponseV1>(new
            {
                GotIt = context.Message.Timestamp
            });
        }
    }
}
