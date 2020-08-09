using FluentValidation;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Validators
{
    public class ClientInfoSmValidator : AbstractValidator<ClientInfoSm>
    {
        public ClientInfoSmValidator()
        {
            RuleFor(e => e.Timestamp).NotEmpty();
        }
    }
}
