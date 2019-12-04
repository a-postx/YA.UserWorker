using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;
using Microsoft.Extensions.Hosting;

namespace YA.TenantWorker
{
    public static class MvcCoreBuilderExtensions
    {
        /// <summary>
        /// Add cross-origin resource sharing (CORS) services and configures named CORS policies. See
        /// https://docs.asp.net/en/latest/security/cors.html
        /// </summary>
        public static IMvcCoreBuilder AddCustomCors(this IMvcCoreBuilder builder)
        {
            return builder.AddCors(options =>
                {
                    // Create named CORS policies here which you can consume using application.UseCors("PolicyName")
                    // or a [EnableCors("PolicyName")] attribute on your controller or action.
                    options.AddPolicy(
                        CorsPolicyName.AllowAny,
                        x => x
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });
        }

        /// <summary>
        /// Adds customized JSON serializer settings.
        /// </summary>
        public static IMvcCoreBuilder AddCustomJsonOptions(this IMvcCoreBuilder builder, IWebHostEnvironment hostingEnvironment)
        {
            return builder.AddJsonOptions(options =>
                {
                    if (hostingEnvironment.EnvironmentName == Environments.Development)
                    {
                        // Pretty print the JSON in development for easier debugging.
                        options.JsonSerializerOptions.WriteIndented = true;
                    }
                });
        }

        public static IMvcCoreBuilder AddCustomMvcOptions(this IMvcCoreBuilder builder)
        {
            return builder.AddMvcOptions(options =>
                {                    
                    // Controls how controller actions cache content from the appsettings.json file.
                    CacheProfileOptions cacheProfileOptions = builder
                        .Services
                        .BuildServiceProvider()
                        .GetRequiredService<CacheProfileOptions>();

                    foreach (KeyValuePair<string, CacheProfile> keyValuePair in cacheProfileOptions)
                    {
                        options.CacheProfiles.Add(keyValuePair);
                    }

                    MediaTypeCollection jsonInputFormatterMediaTypes = options
                        .InputFormatters
                        .OfType<NewtonsoftJsonInputFormatter>()
                        .First()
                        .SupportedMediaTypes;
                    MediaTypeCollection jsonOutputFormatterMediaTypes = options
                        .OutputFormatters
                        .OfType<NewtonsoftJsonOutputFormatter>()
                        .First()
                        .SupportedMediaTypes;

                    // Add RESTful JSON media type (application/vnd.restful+json) to the JSON input and output formatters.
                    // See http://restfuljson.org/
                    jsonInputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);
                    jsonOutputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);

                    // Add Problem Details media type (application/problem+json) to the JSON input and output formatters.
                    // See https://tools.ietf.org/html/rfc7807
                    jsonOutputFormatterMediaTypes.Insert(0, ContentType.ProblemJson);

                    // Remove string and stream output formatters. These are not useful for an API serving JSON or XML.
                    options.OutputFormatters.RemoveType<StreamOutputFormatter>();
                    options.OutputFormatters.RemoveType<StringOutputFormatter>();

                    // Returns a 406 Not Acceptable if the MIME type in the Accept HTTP header is not valid.
                    options.ReturnHttpNotAcceptable = true;
                });
        }
    }
}
