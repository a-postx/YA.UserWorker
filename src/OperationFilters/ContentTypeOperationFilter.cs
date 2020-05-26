using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace YA.TenantWorker.OperationFilters
{
    /// <summary>
    /// Adds a Swashbuckle <see cref="OpenApiExample"/> to all available operations with a supported content type.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class ContentTypeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Apply the specified operation.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="context">Context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Description = "Used to properly process HTTP request content.",
                    In = ParameterLocation.Header,
                    Name = HeaderNames.ContentType,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Default = new OpenApiString("application/json"),
                        Type = "string",
                    },
                });
        }
    }
}
