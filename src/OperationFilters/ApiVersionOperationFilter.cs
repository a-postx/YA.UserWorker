using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YA.TenantWorker.OperationFilters
{
    /// <summary>
    /// Adds a Swashbuckle API version filter to all available operations.
    /// </summary>
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            ApiVersion apiVersion = context.ApiDescription.GetApiVersion();
            
            if (apiVersion == null)
            {
                return;
            }

            IList<IParameter> parameters = operation.Parameters;

            if (parameters == null)
            {
                operation.Parameters = parameters = new List<IParameter>();
            }
            
            IParameter parameter = parameters.FirstOrDefault(p => p.Name == "api-version");

            if (parameter == null)
            {
                parameter = new NonBodyParameter()
                {
                    Name = "api-version",
                    Required = true,
                    Default = apiVersion.ToString(),
                    In = "query",
                    Type = "string",
                };
                parameters.Add(parameter);
            }
            else if (parameter is NonBodyParameter pathParameter)
            {
                pathParameter.Default = apiVersion.ToString();
            }

            parameter.Description = "The requested API version";
        }
    }
}
