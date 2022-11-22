namespace YA.UserWorker.Application.Models.SaveModels;

/// <summary>
/// Информация о регистрации, модель сохранения.
/// </summary>
public class UserRegistrationInfoSm
{
    /// <summary>
    /// Токен доступа.
    /// </summary>
    public string AccessToken { get; set; }
    /// <summary>
    /// Токен присоединения к существующему арендатору.
    /// </summary>
    public Guid? JoinTeamToken { get; set; }
}
