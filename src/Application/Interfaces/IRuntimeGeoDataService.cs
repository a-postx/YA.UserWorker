namespace YA.UserWorker.Application.Interfaces;

public enum RuntimeCountry
{
    UN = 0,
    RU = 1,
    CN = 2,
    US = 3,
    DE = 4,
    FR = 5,
    IE = 6,
    GB = 7,
    SG = 8,
    NL = 9
}
/// <summary>
/// Retrieves geodata for the current application.
/// </summary>
public interface IRuntimeGeoDataService
{
    /// <summary>
    /// Get country code of the application location (ISO 3166).
    /// </summary>
    Task<RuntimeCountry> GetCountryCodeAsync(CancellationToken cancellationToken);
}
