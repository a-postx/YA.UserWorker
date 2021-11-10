namespace YA.UserWorker.Application.Models.SaveModels;

/// <summary>
/// Настройки пользователя, модель сохранения.
/// </summary>
public class UserSettingSm
{
    /// <summary>
    /// Признак необходимости показа страницы регистрации.
    /// </summary>
    public bool ShowGettingStarted { get; set; }
}
