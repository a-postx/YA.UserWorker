namespace YA.TenantWorker.Core.Entities
{
    public interface ITenantEntity
    {
        Tenant Tenant { get; set; }
        byte[] tstamp { get; set; }
    }
}
