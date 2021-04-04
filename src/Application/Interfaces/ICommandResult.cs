using FluentValidation.Results;
using YA.UserWorker.Application.Enums;

namespace YA.UserWorker.Application.Interfaces
{
    public interface ICommandResult<TResult>
    {
        public CommandStatus Status { get; }
        public TResult Data { get; }
        public ValidationResult ValidationResult { get; }
    }
}
