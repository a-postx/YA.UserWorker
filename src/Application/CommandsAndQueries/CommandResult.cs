﻿
using FluentValidation.Results;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Application.CommandsAndQueries
{
    public class CommandResult<TResult> : ICommandResult<TResult>
    {
        private CommandResult() { }

        public CommandResult(CommandStatuses status, TResult data, ValidationResult validationResult = null)
        {
            Status = status;
            Data = data;
            ValidationResult = validationResult;
        }

        public CommandStatuses Status { get; protected set; }
        public TResult Data { get; protected set; }
        public ValidationResult ValidationResult { get; protected set; }
    }
}
