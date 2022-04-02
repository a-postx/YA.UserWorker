using Microsoft.Extensions.Options;

namespace YA.UserWorker.Infrastructure.Authentication;

public class AuthenticationOptionsValidator : IValidateOptions<AuthenticationOptions>
{
    public ValidateOptionsResult Validate(string name, AuthenticationOptions options)
    {
        List<string> failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiGatewayHost))
        {
            failures.Add($"{nameof(options.ApiGatewayHost)} option is not found.");
        }

        if (options.ApiGatewayPort == 0)
        {
            failures.Add($"{nameof(options.ApiGatewayPort)} option is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            failures.Add($"{nameof(options.Authority)} option is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add($"{nameof(options.Audience)} option is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.OpenIdConfigurationEndpoint))
        {
            failures.Add($"{nameof(options.OpenIdConfigurationEndpoint)} option is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.LoginRedirectPath))
        {
            failures.Add($"{nameof(options.LoginRedirectPath)} option is not found.");
        }

        if (options.TokenValidationParameters == null)
        {
            failures.Add($"{nameof(options.TokenValidationParameters)} option is not found.");
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
