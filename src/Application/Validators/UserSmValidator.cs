using FluentValidation;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.Validators;

public class UserSmValidator : AbstractValidator<UserSm>
{
    public UserSmValidator()
    {
        RuleFor(e => e.Name).NotEmpty();
        RuleFor(e => e.Email).NotEmpty().Length(5, 128).EmailAddress();

        When(e => e.Settings != null, () => {
            RuleFor(e => e.Settings.ShowGettingStarted).NotNull();
        });
    }
}
