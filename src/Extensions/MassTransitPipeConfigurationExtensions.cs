using GreenPipes;
using YA.UserWorker.Infrastructure.Messaging.Filters;

namespace YA.UserWorker.Extensions;

public static class MassTransitPipeConfiguratorExtensions
{
    /// <summary>
    /// Injects filter to intercept custom message context from MassTransit message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configurator"></param>
    public static void UseMbContextFilter<T>(this IPipeConfigurator<T> configurator) where T : class, PipeContext
    {
        ArgumentNullException.ThrowIfNull(configurator);

        configurator.AddPipeSpecification(new MbMessageContextFilterPipeSpecification<T>());
    }
}
