namespace YA.TenantWorker.Models
{
    public interface ITenantEntity
    {
        Tenant Tenant { get; set; }
        byte[] tstamp { get; set; }
    }
}
