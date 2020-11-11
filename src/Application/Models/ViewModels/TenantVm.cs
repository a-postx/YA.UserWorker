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
        public string Name { get; set; }

        /// <summary>
        /// URL used to retrieve the resource conforming to REST'ful JSON http://restfuljson.org/.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Current pricing tier
        /// </summary>
        public PricingTierVm PricingTier { get; set; }

        /// <summary>
        /// Date the pricing tier is valid for
        /// </summary>
        public DateTime PricingTierActivatedUntil { get; set; }

        /// <summary>
        /// Дата создания объекта.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Дата последней модификации объекта.
        /// </summary>
        public DateTime LastModifiedDateTime { get; set; }
    }
}
