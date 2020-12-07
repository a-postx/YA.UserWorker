using FluentValidation.Results;
using YA.TenantWorker.Application.Enums;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface ICommandResult<TResult>
    {
        public CommandStatus Status { get; }
        public TResult Data { get; }
        public ValidationResult ValidationResult { get; }
    }
}
