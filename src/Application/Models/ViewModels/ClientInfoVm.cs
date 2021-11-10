namespace YA.UserWorker.Application.Models.ViewModels;

/// <summary>
/// Событие публикации информации о клиенте.
/// </summary>
public class ClientInfoVm
{
    private ClientInfoVm() { }
    public ClientInfoVm(bool success)
    {
        Success = success;
    }

    /// <summary>
    /// Результат публикации события.
    /// </summary>
    public bool Success { get; set; }
}
