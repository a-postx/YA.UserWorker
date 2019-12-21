namespace YA.TenantWorker.Core.Entities
{
    public interface IRowVersionedEntity
    {
        byte[] tstamp { get; set; }
    }
}
