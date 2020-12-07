using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Application.Features.Tenants.Queries
{
    public class GetTenantAllPageCommand : IRequest<ICommandResult<PaginatedResult<Tenant>>>
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

        public class GetTenantAllPageHandler : IRequestHandler<GetTenantAllPageCommand, ICommandResult<PaginatedResult<Tenant>>>
        {
            public GetTenantAllPageHandler(ILogger<GetTenantAllPageHandler> logger,
                ITenantWorkerDbContext dbContext,
                IOptions<GeneralOptions> options)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _generalOptions = options.Value;
            }

            private readonly ILogger<GetTenantAllPageHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly GeneralOptions _generalOptions;

            public async Task<ICommandResult<PaginatedResult<Tenant>>> Handle(GetTenantAllPageCommand command, CancellationToken cancellationToken)
            {
                PageOptions pageOptions = command.Options;

                if (pageOptions == null)
                {
                    return new CommandResult<PaginatedResult<Tenant>>(CommandStatus.BadRequest, null);
                }

                pageOptions.First = !pageOptions.First.HasValue && !pageOptions.Last.HasValue ? _generalOptions.DefaultPaginationPageSize : pageOptions.First;

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
                    return new CommandResult<PaginatedResult<Tenant>>(CommandStatus.NotFound, null);
                }


                PaginatedResult<Tenant> result = new PaginatedResult<Tenant>(
                    hasNextPage,
                    hasPreviousPage,
                    totalCount,
                    items
                );

                return new CommandResult<PaginatedResult<Tenant>>(CommandStatus.Ok, result);
            }
        }
    }
}
