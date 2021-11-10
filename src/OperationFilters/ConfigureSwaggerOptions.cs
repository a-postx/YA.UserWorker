using System.Reflection;
using Delobytes.AspNetCore.Swagger;
using Delobytes.AspNetCore.Swagger.OperationFilters;
using Delobytes.AspNetCore.Swagger.SchemaFilters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using YA.UserWorker.Options;

namespace YA.UserWorker.OperationFilters;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider,
        IOptions<AppSecrets> secretOptions,
        IOptions<IdempotencyOptions> idempotencyOptions)
    {
        _provider = provider;
        _secrets = secretOptions.Value;
        _idempotencyOptions = idempotencyOptions.Value;
    }

    private readonly IApiVersionDescriptionProvider _provider;
    private readonly AppSecrets _secrets;
    private readonly IdempotencyOptions _idempotencyOptions;

    public void Configure(SwaggerGenOptions options)
    {
        //можно поставить значение Bearer и входить по простым токенам, как показано ниже
        string swaggerAuthenticationSchemeName = "oauth2";

        //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //{
        //    In = ParameterLocation.Header,
        //    Description = "Заголовок JWT \"Authorization\" используя схему Bearer. Пример: \"Bearer {token}\"",
        //    Name = "Authorization",
        //    Type = SecuritySchemeType.ApiKey,
        //    Scheme = "bearer",
        //    BearerFormat = "JWT"
        //});

        Assembly assembly = typeof(Startup).Assembly;

        options.DescribeAllParametersInCamelCase();
        options.EnableAnnotations();

        // Add the XML comment file for this assembly, so its contents can be displayed.
        options.IncludeXmlCommentsIfExists(assembly);

        options.OperationFilter<ApiVersionOperationFilter>();

        if (_idempotencyOptions.IdempotencyFilterEnabled.HasValue && _idempotencyOptions.IdempotencyFilterEnabled.Value)
        {
            options.OperationFilter<IdempotencyKeyOperationFilter>(_idempotencyOptions.IdempotencyHeader);
        }

        options.OperationFilter<ContentTypeOperationFilter>(true);
        options.OperationFilter<ClaimsOperationFilter>(swaggerAuthenticationSchemeName);
        options.OperationFilter<SecurityRequirementsOperationFilter>(true, swaggerAuthenticationSchemeName);

        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = swaggerAuthenticationSchemeName
                            }
                        },
                        Array.Empty<string>()
                    }
                });

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows()
            {
                Implicit = new OpenApiOAuthFlow()
                {
                    AuthorizationUrl = new Uri(_secrets.OauthImplicitAuthorizationUrl),
                    TokenUrl = new Uri(_secrets.OauthImplicitTokenUrl),
                    Scopes = new Dictionary<string, string>()
                }
            }
        });

        // Show an example model for JsonPatchDocument<T>.
        options.SchemaFilter<JsonPatchDocumentSchemaFilter>();

        string assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        string assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        foreach (ApiVersionDescription apiVersionDescription in _provider.ApiVersionDescriptions)
        {
            OpenApiInfo info = new OpenApiInfo()
            {
                Title = assemblyProduct,
                Description = apiVersionDescription.IsDeprecated
                    ? $"{assemblyDescription} This API version has been deprecated."
                    : assemblyDescription,
                Version = apiVersionDescription.ApiVersion.ToString()
            };
            options.SwaggerDoc(apiVersionDescription.GroupName, info);
        }
    }
}
