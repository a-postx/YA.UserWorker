
using FluentValidation.Results;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Application.Features;

public class CommandResult<TResult> : ICommandResult<TResult>
{
    private CommandResult() { }

    public CommandResult(CommandStatus status, TResult data, ValidationResult validationResult = null)
    {
        Status = status;
        Data = data;
        ValidationResult = validationResult;
    }

    public CommandStatus Status { get; protected set; }
    public TResult Data { get; protected set; }
    public ValidationResult ValidationResult { get; protected set; }
}
