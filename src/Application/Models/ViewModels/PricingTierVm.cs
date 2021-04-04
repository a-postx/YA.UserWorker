using System;

namespace YA.UserWorker.Application.Models.ViewModels
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
    }
}
