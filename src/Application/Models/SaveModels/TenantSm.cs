namespace YA.TenantWorker.Application.Models.SaveModels
{
    /// <summary>
    /// Арендатор, модель сохранения.
    /// </summary>
    public class TenantSm
    {
        /// <summary>
        /// Имя арендатора.
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Сигнал к активации арендатора.
        /// </summary>
        public bool IsActive { get; set; }
    }
}