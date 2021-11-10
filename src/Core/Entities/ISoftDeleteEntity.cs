namespace YA.UserWorker.Core.Entities;

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; set; }
}
