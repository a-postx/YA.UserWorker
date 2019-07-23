using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.OperationFilters
{
    /// <summary>
    /// Adds a Swashbuckle <see cref="NonBodyParameter"/> to all available operations with a description of X-Correlation-ID
    /// HTTP header and default GUID value.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class CorrelationIdOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Apply the specified operation.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="context">Context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }

            operation.Parameters.Add(
                new NonBodyParameter()
                {
                    Default = Guid.NewGuid(),
                    Description = "Used to identify HTTP request: the ID will correlate HTTP request between server and client.",
                    In = "header",
                    Name = General.CorrelationIdHeader,
                    Required = false,
                    Type = "string",
                });
        }
    }
}
