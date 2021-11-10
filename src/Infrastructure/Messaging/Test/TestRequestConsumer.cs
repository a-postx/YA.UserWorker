using MassTransit;

namespace YA.UserWorker.Infrastructure.Messaging.Test;

public class TestRequestConsumer : IConsumer<IUserWorkerTestRequestV1>
{
    public TestRequestConsumer()
    {

    }

    public async Task Consume(ConsumeContext<IUserWorkerTestRequestV1> context)
    {
        await context.RespondAsync<IUserWorkerTestResponseV1>(new
        {
            GotIt = context.Message.Timestamp
        });
    }
}
