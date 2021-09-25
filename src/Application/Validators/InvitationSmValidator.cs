using FluentValidation;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.Validators
{
    public class InvitationSmValidator : AbstractValidator<InvitationSm>
    {
        public InvitationSmValidator()
        {
            RuleFor(e => e.Email).NotEmpty();
            RuleFor(e => e.AccessType).IsInEnum().NotEqual(MembershipAccessType.None);
            RuleFor(e => e.InvitedBy).NotEmpty();
        }
    }
}
