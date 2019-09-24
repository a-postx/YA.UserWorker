using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Models.ViewModelSchemaFilters
{
    public class TenantSmSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            TenantSm tenantSm = new TenantSm()
            {
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                TenantName = "Yo Code LLC"
            };

            model.Default = tenantSm;
            model.Example = tenantSm;
        }
    }
}
