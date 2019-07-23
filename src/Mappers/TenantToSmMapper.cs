using System;
using Delobytes.Mapper;
using YA.TenantWorker.Models;
using YA.TenantWorker.SaveModels;

namespace YA.TenantWorker.Mappers
{
    /// <summary>
    /// Mapper for mapping internal tenant object into savetenant and vice versa.
    /// </summary>
    public class TenantToSmMapper : IMapper<Tenant, TenantSm>, IMapper<TenantSm, Tenant>
    {
        public TenantToSmMapper()
        {
            
        }

        public void Map(Tenant source, TenantSm destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.TenantId = source.TenantID;
            destination.TenantName = source.TenantName;
        }

        public void Map(TenantSm source, Tenant destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.TenantID = source.TenantId;
            destination.TenantName = source.TenantName;
        }
    }
}
