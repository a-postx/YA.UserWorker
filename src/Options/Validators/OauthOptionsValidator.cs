using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace YA.UserWorker.Options.Validators
{
    public class OauthOptionsValidator : IValidateOptions<OauthOptions>
    {
        public ValidateOptionsResult Validate(string name, OauthOptions options)
        {
            List<string> failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.Authority))
            {
                failures.Add($"{nameof(options.Authority)} option is not found.");
            }

            if (string.IsNullOrWhiteSpace(options.ClientId))
            {
                failures.Add($"{nameof(options.ClientId)} option is not found.");
            }

            if (string.IsNullOrWhiteSpace(options.Audience))
            {
                failures.Add($"{nameof(options.Audience)} option is not found.");
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
