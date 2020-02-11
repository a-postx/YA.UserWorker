using System;
using Delobytes.Mapper;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.Mappers
{
    /// <summary>
    /// Mapper for mapping internal pricing tier object into transfer model and vice versa.
    /// </summary>
    public class PricingTierToTmMapper : IMapper<PricingTier, PricingTierTm>, IMapper<PricingTierTm, PricingTier>
    {
        public PricingTierToTmMapper()
        {

        }

        public void Map(PricingTier source, PricingTierTm destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.HasTrial = source.HasTrial;
            destination.TrialPeriod = source.TrialPeriod;
            destination.MaxScheduledTasks = source.MaxScheduledTasks;
            destination.MaxUsers = source.MaxUsers;
            destination.MaxVkCommunities = source.MaxVkCommunities;
            destination.MaxVkCommunitySize = source.MaxVkCommunitySize;
        }

        public void Map(PricingTierTm source, PricingTier destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.HasTrial = source.HasTrial;
            destination.TrialPeriod = source.TrialPeriod;
            destination.MaxScheduledTasks = source.MaxScheduledTasks;
            destination.MaxUsers = source.MaxUsers;
            destination.MaxVkCommunities = source.MaxVkCommunities;
            destination.MaxVkCommunitySize = source.MaxVkCommunitySize;
        }
    }
}
