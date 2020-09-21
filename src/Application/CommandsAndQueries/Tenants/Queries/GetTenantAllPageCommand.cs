using Delobytes.Mapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.BusinessModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.CommandsAndQueries.Tenants.Queries
{
    public class GetTenantAllPageCommand : IRequest<ICommandResult<PaginatedResultBm<TenantVm>>>
    {
        public GetTenantAllPageCommand(PageOptions pageOptions, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore)
        {
            Options = pageOptions;
            CreatedAfter = createdAfter;
            CreatedBefore = createdBefore;
        }

        public PageOptions Options { get; protected set; }
        public DateTimeOffset? CreatedAfter { get; protected set; }
        public DateTimeOffset? CreatedBefore { get; protected set; }

        public class GetTenantAllPageHandler : IRequestHandler<GetTenantAllPageCommand, ICommandResult<PaginatedResultBm<TenantVm>>>
        {
            public GetTenantAllPageHandler(ILogger<GetTenantAllPageHandler> logger,
                ITenantWorkerDbContext dbContext,
                IMapper<Tenant, TenantVm> tenantVmMapper)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            }

            private readonly ILogger<GetTenantAllPageHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

            public async Task<ICommandResult<PaginatedResultBm<TenantVm>>> Handle(GetTenantAllPageCommand command, CancellationToken cancellationToken)
            {
                PageOptions pageOptions = command.Options;

                if (pageOptions == null)
                {
                    return new CommandResult<PaginatedResultBm<TenantVm>>(CommandStatuses.BadRequest, null);
                }

                pageOptions.First = !pageOptions.First.HasValue && !pageOptions.Last.HasValue ? General.DefaultPageSizeForPagination : pageOptions.First;

                Task<List<Tenant>> getItemsTask = _dbContext
                    .GetEntitiesPagedTaskAsync<Tenant>(pageOptions.First, pageOptions.Last, command.CreatedAfter, command.CreatedBefore, cancellationToken);
                Task<bool> getHasNextPageTask = _dbContext
                    .GetHasNextPageAsync<Tenant>(pageOptions.First, command.CreatedAfter, command.CreatedBefore, cancellationToken);
                Task<bool> getHasPreviousPageTask = _dbContext
                    .GetHasPreviousPageAsync<Tenant>(pageOptions.Last, command.CreatedAfter, command.CreatedBefore, cancellationToken);
                Task<int> totalCountTask = _dbContext.GetEntitiesCountAsync<Tenant>(cancellationToken);

                await Task.WhenAll(getItemsTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);

                List<Tenant> items = await getItemsTask;
                bool hasNextPage = await getHasNextPageTask;
                bool hasPreviousPage = await getHasPreviousPageTask;
                int totalCount = await totalCountTask;

                if (items == null)
                {
                    return new CommandResult<PaginatedResultBm<TenantVm>>(CommandStatuses.NotFound, null);
                }

                List<TenantVm> itemVms = _tenantVmMapper.MapList(items);

                PaginatedResultBm<TenantVm> result = new PaginatedResultBm<TenantVm>(
                    hasNextPage,
                    hasPreviousPage,
                    totalCount,
                    itemVms
                );

                return new CommandResult<PaginatedResultBm<TenantVm>>(CommandStatuses.Ok, result);
            }
        }
    }
}
