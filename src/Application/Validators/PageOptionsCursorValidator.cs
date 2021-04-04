using FluentValidation;
using YA.UserWorker.Application.Models.HttpQueryParams;

namespace YA.UserWorker.Application.Validators
{
    public class PageOptionsCursorValidator : AbstractValidator<PageOptionsCursor>
    {
        public PageOptionsCursorValidator()
        {
            RuleFor(e => e.First).InclusiveBetween(1, 50);
            RuleFor(e => e.Last).InclusiveBetween(1, 50);
        }
    }
}
