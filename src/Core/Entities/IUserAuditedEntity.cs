namespace YA.UserWorker.Core.Entities;

public interface IUserAuditedEntity
{
    string CreatedBy { get; set; }
    string LastModifiedBy { get; set; }
}
