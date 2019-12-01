﻿using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using YA.TenantWorker.Application.Models.SaveModels;
using Newtonsoft.Json;

namespace YA.TenantWorker.Application.Models.ViewModelSchemaFilters
{
    public class TenantSmSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            TenantSm tenantSm = new TenantSm()
            {
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                TenantName = "Yo Code LLC"
            };

            model.Default = new OpenApiString(JsonConvert.SerializeObject(tenantSm, Formatting.Indented));
            model.Example = new OpenApiString(JsonConvert.SerializeObject(tenantSm, Formatting.Indented));
        }
    }
}
