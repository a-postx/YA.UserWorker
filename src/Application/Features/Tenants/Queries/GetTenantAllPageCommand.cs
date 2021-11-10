using MediatR;
using Microsoft.Extensions.Options;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core;
using YA.UserWorker.Core.Entities;
using YA.UserWorker.Options;

namespace YA.UserWorker.Application.Features.Tenants.Queries;

public class GetTenantAllPageCommand : IRequest<ICommandResult<CursorPaginatedResult<Tenant>>>
{
    public GetTenantAllPageCommand(int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore)
    {
        First = first;
        Last = last;
        CreatedAfter = createdAfter;
        CreatedBefore = createdBefore;
    }

    public int? First { get; protected set; }
    public int? Last { get; protected set; }
    public DateTimeOffset? CreatedAfter { get; protected set; }
    public DateTimeOffset? CreatedBefore { get; protected set; }

    public class GetTenantAllPageHandler : IRequestHandler<GetTenantAllPageCommand, ICommandResult<CursorPaginatedResult<Tenant>>>
    {
        public GetTenantAllPageHandler(ILogger<GetTenantAllPageHandler> logger,
            IUserWorkerDbContext dbContext,
            IOptions<GeneralOptions> options)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _generalOptions = options.Value;
        }

        private readonly ILogger<GetTenantAllPageHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;
        private readonly GeneralOptions _generalOptions;

        public async Task<ICommandResult<CursorPaginatedResult<Tenant>>> Handle(GetTenantAllPageCommand command, CancellationToken cancellationToken)
        {
            int? first = command.First;
            int? last = command.Last;

            first = !first.HasValue && !last.HasValue ? _generalOptions.DefaultPaginationPageSize : first;

            Task<List<Tenant>> getItemsTask = _dbContext
                .GetEntitiesPagedTaskAsync<Tenant>(first, last, command.CreatedAfter, command.CreatedBefore, cancellationToken);
            Task<bool> getHasNextPageTask = _dbContext
                .GetHasNextPageAsync<Tenant>(first, command.CreatedAfter, command.CreatedBefore, cancellationToken);
            Task<bool> getHasPreviousPageTask = _dbContext
                .GetHasPreviousPageAsync<Tenant>(last, command.CreatedAfter, command.CreatedBefore, cancellationToken);
            Task<int> totalCountTask = _dbContext.GetEntitiesCountAsync<Tenant>(cancellationToken);

            await Task.WhenAll(getItemsTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);

            List<Tenant> items = await getItemsTask;
            bool hasNextPage = await getHasNextPageTask;
            bool hasPreviousPage = await getHasPreviousPageTask;
            int totalCount = await totalCountTask;

            if (items == null)
            {
                return new CommandResult<CursorPaginatedResult<Tenant>>(CommandStatus.NotFound, null);
            }

            CursorPaginatedResult<Tenant> result = new CursorPaginatedResult<Tenant>(
                hasNextPage,
                hasPreviousPage,
                totalCount,
                items
            );

            return new CommandResult<CursorPaginatedResult<Tenant>>(CommandStatus.Ok, result);
        }
    }
}
