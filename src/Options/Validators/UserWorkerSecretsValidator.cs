using Microsoft.Extensions.Options;

namespace YA.UserWorker.Options.Validators;

public class UserWorkerSecretsValidator : IValidateOptions<UserWorkerSecrets>
{
    public ValidateOptionsResult Validate(string name, UserWorkerSecrets options)
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
