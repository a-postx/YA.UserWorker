using System;

namespace YA.TenantWorker.Application.Models.ViewModels
{
    /// <summary>
    /// Тарифный план, обзорная модель.
    /// </summary>
    public class PricingTierVm
    {
        /// <summary>
        /// Идентификатор тарифного плана.
        /// </summary>
        public Guid PricingTierId { get; set; }

        /// <summary>
        /// Название тарифного плана.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Максимальное число сообществ ВКонтакте.
        /// </summary>
        public int MaxVkCommunities { get; set; }
    }
}
