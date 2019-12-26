using System;

namespace YA.TenantWorker.Application.Models.ViewModels
{
    /// <summary>
    /// Tenant view model.
    /// </summary>
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
        public Uri Url { get; set; }
    }
}
