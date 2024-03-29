namespace YA.UserWorker.Core.Entities;

public enum YaTenantType
{
    System = 0,
    Custom = 1
}

public enum YaTenantStatus
{
    New = 0,
    Active = 1
}

/// <summary>
/// Арендатор - базовая сущность, обозначающая определённое рабочее пространство. Является внешним ключём
/// у всех создаваемых пользователями сущностей.
/// </summary>
public class Tenant : IRowVersionedEntity, IUserAuditedEntity, IAuditedEntityBase
{
    public Guid TenantID { get; set; }
    public YaTenantType Type { get; set; }
    public string Name { get; set; }
    public Guid PricingTierId { get; set; }
    public virtual PricingTier PricingTier { get; set; }
    public DateTime PricingTierActivatedDateTime { get; set; }
    public DateTime PricingTierActivatedUntilDateTime { get; set; }
    public YaTenantStatus Status { get; set; }
    public bool IsReadOnly { get; set; }
    public virtual ICollection<Membership> Memberships { get; set; }
    public virtual ICollection<YaInvitation> Invitations { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastModifiedDateTime { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public byte[] tstamp { get; set; }
}
