namespace YA.UserWorker.Core.Entities;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
    Tenant Tenant { get; set; }
}
