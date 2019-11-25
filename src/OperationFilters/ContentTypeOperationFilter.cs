using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace YA.TenantWorker.OperationFilters
{
    /// <summary>
    /// Adds a Swashbuckle <see cref="NonBodyParameter"/> to all available operations
    /// HTTP header and default GUID value.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class ContentTypeOperationFilter : IOperationFilter
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
                    Default = "application/json",
                    Description = "Used to properly process HTTP request content.",
                    In = "header",
                    Name = HeaderNames.ContentType,
                    Required = true,
                    Type = "string",
                });
        }
    }
}
