using FluentValidation;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.Validators
{
    public class ClientInfoSmValidator : AbstractValidator<ClientInfoSm>
    {
        public ClientInfoSmValidator()
        {
            RuleFor(e => e.Timestamp).NotEmpty();
        }
    }
}
