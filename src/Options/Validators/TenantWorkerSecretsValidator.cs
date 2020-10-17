using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace YA.TenantWorker.Options.Validators
{
    public class TenantWorkerSecretsValidator : IValidateOptions<TenantWorkerSecrets>
    {
        public ValidateOptionsResult Validate(string name, TenantWorkerSecrets options)
        {
            List<string> failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                failures.Add($"{nameof(options.ConnectionString)} secret is not found.");
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
