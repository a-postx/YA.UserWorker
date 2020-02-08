namespace YA.TenantWorker.Core.Entities
{
    public interface ISoftDeleteEntity
    {
        bool IsDeleted { get; set; }
    }
}
