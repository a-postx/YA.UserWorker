namespace YA.UserWorker.Application.Models.ViewModels;

/// <summary>
/// Тарифный план, визуальная модель.
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
    /// Ограничение максимального количества пользователей.
    /// </summary>
    public int MaxUsers { get; set; }

    /// <summary>
    /// Ограничение максимального количества периодических задач парсинга.
    /// </summary>
    public int MaxVkPeriodicParsingTasks { get; set; }
}
