using FluentValidation;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.Validators
{
    public class AccessInfoSmValidator : AbstractValidator<UserRegistrationInfoSm>
    {
        public AccessInfoSmValidator()
        {
            RuleFor(e => e.AccessToken).NotEmpty();
        }
    }
}
