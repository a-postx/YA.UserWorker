namespace YA.UserWorker.Core.Entities;

public interface IRowVersionedEntity
{
    byte[] tstamp { get; set; }
}
