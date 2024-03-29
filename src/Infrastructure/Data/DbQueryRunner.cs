using Microsoft.EntityFrameworkCore;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Infrastructure.Data;

public class DbQueryRunner : IDbQueryRunner
{
    public DbQueryRunner(UserWorkerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private bool _disposed;
    private readonly UserWorkerDbContext _context;

    public Task RunQueryAsync(string query, params object[] parameters)
    {
        return _context.Database.ExecuteSqlRawAsync(query, parameters);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DbQueryRunner()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // free other managed objects that implement
            // IDisposable only
        }

        _context?.Dispose();
        // release any unmanaged objects
        // set thick object references to null

        _disposed = true;
    }
}
