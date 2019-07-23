using System;
using System.Threading.Tasks;
using MassTransit;

namespace YA.TenantWorker.MessageBus
{
    public class TestRequestConsumer : IConsumer<ITenantManagerTestRequestV1>
    {
        public TestRequestConsumer()
        {

        }

        public async Task Consume(ConsumeContext<ITenantManagerTestRequestV1> context)
        {
            await context.RespondAsync<ITenantManagerTestResponseV1>(new
            {
                GotIt = context.Message.Timestamp
            });
        }
    }
}
