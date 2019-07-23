using System;
using YA.TenantWorker.ViewModelSchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace YA.TenantWorker.ViewModels
{
    /// <summary>
    /// Tenant view model.
    /// </summary>
    [SwaggerSchemaFilter(typeof(TenantVmSchemaFilter))]
    public class TenantVm
    {
        /// <summary>
        /// Tenant unique identifier.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Tenant name.
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// URL used to retrieve the resource conforming to REST'ful JSON http://restfuljson.org/.
        /// </summary>
        public string Url { get; set; }
    }
}
