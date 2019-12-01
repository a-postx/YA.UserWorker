using System;
using YA.TenantWorker.Application.Models.ViewModels;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;

namespace YA.TenantWorker.Application.Models.ViewModelSchemaFilters
{
    public class TenantVmSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            Guid tenantId = Guid.NewGuid();

            TenantVm tenant = new TenantVm()
            {
                TenantId = tenantId,
                TenantName = "MyCoolTenant",
                Url = $"/tenants/{tenantId}"
            };

            model.Default = new OpenApiString(JsonConvert.SerializeObject(tenant, Formatting.Indented));
            model.Example = new OpenApiString(JsonConvert.SerializeObject(tenant, Formatting.Indented));
        }
    }
}
