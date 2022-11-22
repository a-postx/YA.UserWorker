namespace YA.UserWorker.Core.Entities;

public interface IRowVersionedEntity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Имя принято в среде EFCore")]
    byte[] tstamp { get; set; }
}
