namespace YA.UserWorker.Application.Interfaces;

public interface IDbQueryRunner : IDisposable
{
    Task RunQueryAsync(string query, params object[] parameters);
}
