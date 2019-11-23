using GreenPipes;
using System.Collections.Generic;
using System.Linq;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    class YaMbEventSerilogEnricherSpecification<T> : IPipeSpecification<T> where T : class, PipeContext
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new YaMbEventSerilogEnricherFilter<T>());
        }
    }
}
