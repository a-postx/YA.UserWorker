namespace YA.UserWorker.Infrastructure.Health.System;

/// <summary>
/// Сетевые настройки проверки здоровья.
/// </summary>
public class NetworkCheckOptions
{
    public int MaxLatencyThreshold { get; set; } = 500;
    public string InternetHost { get; } = "77.88.8.8";
}
