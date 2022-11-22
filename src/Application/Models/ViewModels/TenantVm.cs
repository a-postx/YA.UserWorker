namespace YA.UserWorker.Application.Models.ViewModels;

/// <summary>
/// Арендатор, визуальная модель.
/// </summary>
public class TenantVm
{
    /// <summary>
    /// Уникальный идентификатор.
    /// </summary>
    public Guid TenantId { get; set; }
        
    /// <summary>
    /// Имя арендатора.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Адрес сущности согласно спецификации http://restfuljson.org/.
    /// </summary>
    public Uri Url { get; set; }

    /// <summary>
    /// Идентификатор тарифного плана.
    /// </summary>
    public Guid PricingTierId { get; set; }

    /// <summary>
    /// Текущий тарифный план.
    /// </summary>
    public PricingTierVm PricingTier { get; set; }

    /// <summary>
    /// Дата активации тарифного плана.
    /// </summary>
    public DateTime PricingTierActivatedDateTime { get; set; }

    /// <summary>
    /// Дата истечения тарифного плана.
    /// </summary>
    public DateTime PricingTierActivatedUntilDateTime { get; set; }

    /// <summary>
    /// Членства пользователей в арендаторе.
    /// </summary>
    public ICollection<MembershipVm> Memberships { get; set; }

    /// <summary>
    /// Приглашения пользователей в арендатор.
    /// </summary>
    public ICollection<InvitationVm> Invitations { get; set; }
}
