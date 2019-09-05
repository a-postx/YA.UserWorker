using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Dto.ViewModelSchemaFilters;

namespace YA.TenantWorker.Application.Dto.SaveModels
{
    /// <summary>
    /// Tenant model from external API call.
    /// </summary>
    [SwaggerSchemaFilter(typeof(TenantSmSchemaFilter))]
    public class TenantSm
    {
        /// <summary>
        /// Tenant unique identifier.
        /// </summary>
        public Guid TenantId { get; set; }
        /// <summary>
        /// Tenant name.
        /// </summary>
        public string TenantName { get; set; }
    }
}
