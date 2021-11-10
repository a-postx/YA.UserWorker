namespace YA.UserWorker.Core.Entities;

public interface IAuditedEntityBase
{
    DateTime CreatedDateTime { get; set; }
    DateTime LastModifiedDateTime { get; set; }
}
