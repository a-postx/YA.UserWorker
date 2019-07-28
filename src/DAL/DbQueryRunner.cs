using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YA.TenantWorker.DAL
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
