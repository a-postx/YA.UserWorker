using FluentValidation;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.Validators
{
    public class PageOptionsValidator : AbstractValidator<PageOptions>
    {
        public PageOptionsValidator()
        {
            RuleFor(e => e.First).InclusiveBetween(1, 50);
            RuleFor(e => e.Last).InclusiveBetween(1, 50);
        }
    }
}
