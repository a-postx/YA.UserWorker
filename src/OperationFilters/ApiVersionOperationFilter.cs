using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YA.UserWorker.OperationFilters
{
    /// <summary>
    /// Добавляет фильтр для документирования параметра предполагаемой версии АПИ.
    /// </summary>
    /// <remarks>Фильтр <see cref="IOperationFilter"/> требуется только из-за багов в <see cref="SwaggerGenerator"/>.
    /// Когда починят и опубликуют, фильтр можно будет убрать. См:
    /// - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
    /// - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413</remarks>
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ApiDescription apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters is null)
            {
                return;
            }

            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                ApiParameterDescription description = apiDescription.ParameterDescriptions
                    .First(x => string.Equals(x.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (parameter.Description is null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default is null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}
