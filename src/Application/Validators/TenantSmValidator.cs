using FluentValidation;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.Validators;

public class TenantSmValidator : AbstractValidator<TenantSm>
{
    public TenantSmValidator()
    {
        RuleFor(e => e.Name).NotEmpty().MaximumLength(64);
    }
}
