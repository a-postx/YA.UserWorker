using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YA.TenantWorker.OperationFilters
{
    /// <summary>
    /// Adds a Swashbuckle API version filter to all available operations.
    /// </summary>
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ApiVersion apiVersion = context.ApiDescription.GetApiVersion();

            if (apiVersion == null)
            {
                return;
            }

            IList<OpenApiParameter> parameters = operation.Parameters;

            if (parameters == null)
            {
                operation.Parameters = parameters = new List<OpenApiParameter>();
            }

            OpenApiParameter parameter = parameters.FirstOrDefault(p => p.Name == "api-version");

            if (parameter == null)
            {
                parameter = new OpenApiParameter()
                {
                    Name = "api-version",
                    Required = true,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Default = new OpenApiString(apiVersion.ToString()),
                        Type = "string"
                    },
                };
                parameters.Add(parameter);
            }
            else if (parameter is OpenApiParameter pathParameter)
            {
                pathParameter.Schema = new OpenApiSchema
                {
                    Default = new OpenApiString(apiVersion.ToString()),
                    Type = "string"
                };
            }

            parameter.Description = "The requested API version";
        }
    }
}
