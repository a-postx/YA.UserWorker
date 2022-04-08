using Microsoft.Extensions.Options;

namespace YA.UserWorker.Options.Validators;

public class AppSecretsValidator : IValidateOptions<AppSecrets>
{
    public ValidateOptionsResult Validate(string name, AppSecrets options)
    {
        List<string> failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ElasticSearchUrl))
        {
            failures.Add($"{nameof(options.ElasticSearchUrl)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.ElasticSearchUser))
        {
            failures.Add($"{nameof(options.ElasticSearchUser)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.ElasticSearchPassword))
        {
            failures.Add($"{nameof(options.ElasticSearchPassword)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.MessageBusHost))
        {
            failures.Add($"{nameof(options.MessageBusHost)} secret is not found.");
        }

        if (options.MessageBusPort <= 0)
        {
            failures.Add($"{nameof(options.MessageBusPort)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.MessageBusVHost))
        {
            failures.Add($"{nameof(options.MessageBusVHost)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.MessageBusLogin))
        {
            failures.Add($"{nameof(options.MessageBusLogin)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.MessageBusPassword))
        {
            failures.Add($"{nameof(options.MessageBusPassword)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.DistributedCacheHost))
        {
            failures.Add($"{nameof(options.DistributedCacheHost)} secret is not found.");
        }

        if (options.DistributedCachePort <= 0)
        {
            failures.Add($"{nameof(options.DistributedCachePort)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.DistributedCachePassword))
        {
            failures.Add($"{nameof(options.DistributedCachePassword)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.Auth0ManagementApiClientId))
        {
            failures.Add($"{nameof(options.Auth0ManagementApiClientId)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.Auth0ManagementApiClientSecret))
        {
            failures.Add($"{nameof(options.Auth0ManagementApiClientSecret)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.KeycloakManagementApiClientId))
        {
            failures.Add($"{nameof(options.KeycloakManagementApiClientId)} secret is not found.");
        }

        if (string.IsNullOrWhiteSpace(options.KeycloakManagementApiClientSecret))
        {
            failures.Add($"{nameof(options.KeycloakManagementApiClientSecret)} secret is not found.");
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
