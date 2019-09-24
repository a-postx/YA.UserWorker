using System;
using YA.TenantWorker.Application.Models.ViewModels;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YA.TenantWorker.Application.Models.ViewModelSchemaFilters
{
    public class TenantVmSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            Guid tenantId = Guid.NewGuid();

            TenantVm tenant = new TenantVm()
            {
                TenantId = tenantId,
                TenantName = "MyCoolTenant",
                Url = $"/tenants/{tenantId}"
            };

            model.Default = tenant;
            model.Example = tenant;
        }
    }
}
