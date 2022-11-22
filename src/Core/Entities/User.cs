namespace YA.UserWorker.Core.Entities;

/// <summary>
/// Пользователь приложения.
/// </summary>
// моделирование многоарендаторных сущностей по опыту работы с Азурой
// и по примеру https://blog.checklyhq.com/building-a-multi-tenant-saas-data-model/
public class User : IAuditedEntityBase, IRowVersionedEntity, ISoftDeleteEntity
{
    public Guid UserID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AuthProvider { get; set; }
    public string ExternalId { get; set; }
    public string Picture { get; set; }
    public string Nickname { get; set; }
    public virtual UserSetting Settings { get; set; }
    public virtual ICollection<Membership> Memberships { get; set; }
    //используется только в визуальных моделях, добавлено для сокращения количества запросов
    public ICollection<Tenant> Tenants { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastModifiedDateTime { get; set; }
    public byte[] tstamp { get; set; }
}
