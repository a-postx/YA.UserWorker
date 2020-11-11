using FluentValidation;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Validators
{
    public class TenantSmValidator : AbstractValidator<TenantSm>
    {
        public TenantSmValidator()
        {
            RuleFor(e => e.Name).NotEmpty();
        }
    }
}
