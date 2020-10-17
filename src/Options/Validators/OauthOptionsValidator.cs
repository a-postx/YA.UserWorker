using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace YA.TenantWorker.Options.Validators
{
    public class OauthOptionsValidator : IValidateOptions<OauthOptions>
    {
        public ValidateOptionsResult Validate(string name, OauthOptions options)
        {
            List<string> failures = new List<string>();

            // время ожидания, пока все фоновые сервисы остановятся
            if (string.IsNullOrWhiteSpace(options.ClientId))
            {
                failures.Add($"{nameof(options.ClientId)} option is not found.");
            }

            if (failures.Count > 0)
            {
                return ValidateOptionsResult.Fail(failures);
            }
            else
            {
                return ValidateOptionsResult.Success;
            }
        }
    }
}