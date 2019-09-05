using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Infrastructure.Data
{
    public class DbQueryRunner : IDbQueryRunner
    {
        public DbQueryRunner(TenantWorkerDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TenantWorkerDbContext Context { get; set; }

        public Task RunQueryAsync(string query, params object[] parameters)
        {
            return Context.Database.ExecuteSqlCommandAsync(query, parameters);
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
