using YA.UserWorker.Application.Enums;

namespace YA.UserWorker.Application.Models.ViewModels;

/// <summary>
/// Приглашение в арендатора, визуальная модель.
/// </summary>
public class InvitationVm
{
    /// <summary>
    /// Идентификатор арендатора
    /// </summary>
    public Guid TenantId { get; set; }
    /// <summary>
    /// Имя арендатора.
    /// </summary>
    public string TenantName { get; set; }
    /// <summary>
    /// Уникальный идентификатор.
    /// </summary>
    public Guid YaInvitationID { get; set; }
    /// <summary>
    /// Пригласивший пользователь.
    /// </summary>
    public string InvitedBy { get; set; }
    /// <summary>
    /// Электропочта.
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Уровень доступа к арендатору.
    /// </summary>
    public MembershipAccessType AccessType { get; set; }
    /// <summary>
    /// Признак использования.
    /// </summary>
    public bool Claimed { get; set; }
    /// <summary>
    /// Дата истечения.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    /// <summary>
    /// Статус.
    /// </summary>
    public TenantInvitationStatus Status { get; set; }
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedDateTime { get; set; }
}
