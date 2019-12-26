using System;

namespace YA.TenantWorker.Application.Models.SaveModels
{
    /// <summary>
    /// Tenant model from external API call.
    /// </summary>
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
