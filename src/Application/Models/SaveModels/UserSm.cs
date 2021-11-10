namespace YA.UserWorker.Application.Models.SaveModels;

/// <summary>
/// Пользователь, модель сохранения.
/// </summary>
public class UserSm
{
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Электропочта пользователя.
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Настройки пользователя.
    /// </summary>
    public UserSettingSm Settings { get; set; }
}
