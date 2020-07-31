using Delobytes.AspNetCore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;

namespace YA.TenantWorker
{
    internal static class MvcBuilderExtensions
    {
        /// <summary>
        /// Adds customized JSON serializer settings.
        /// </summary>
        public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder builder, IWebHostEnvironment webHostEnvironment)
        {
            return builder.AddJsonOptions(options =>
            {
                JsonSerializerOptions jsonSerializerOptions = options.JsonSerializerOptions;
                if (webHostEnvironment.IsDevelopment())
                {
                    // Pretty print the JSON in development for easier debugging.
                    jsonSerializerOptions.WriteIndented = true;
                }

                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                jsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
        }

        public static IMvcBuilder AddCustomMvcOptions(this IMvcBuilder builder, IConfiguration configuration)
        {
            return builder.AddMvcOptions(options =>
            {
                // Controls how controller actions cache content from the appsettings.json file.
                foreach (KeyValuePair<string, CacheProfile> keyValuePair in configuration
                    .GetSection(nameof(ApplicationOptions.CacheProfiles))
                    .Get<CacheProfileOptions>())
                {
                    options.CacheProfiles.Add(keyValuePair);
                }

                // Remove plain text (text/plain) output formatter.
                options.OutputFormatters.RemoveType<StringOutputFormatter>();

                ////MediaTypeCollection xmlInputFormatterMediaTypes = options
                ////    .InputFormatters
                ////    .OfType<XmlDataContractSerializerInputFormatter>()
                ////    .First()
                ////    .SupportedMediaTypes;

                ////MediaTypeCollection xmlOutputFormatterMediaTypes = options
                ////    .OutputFormatters
                ////    .OfType<XmlDataContractSerializerOutputFormatter>()
                ////    .First()
                ////    .SupportedMediaTypes;

                ////// Remove XML text (text/xml) media type from the XML input and output formatters.
                ////xmlInputFormatterMediaTypes.Remove("text/xml");
                ////xmlOutputFormatterMediaTypes.Remove("text/xml");


                // Configure System.Text JSON.
                MediaTypeCollection jsonSystemInputFormatterMediaTypes = options
                        .InputFormatters
                        .OfType<SystemTextJsonInputFormatter>()
                        .First()
                        .SupportedMediaTypes;
                MediaTypeCollection jsonSystemOutputFormatterMediaTypes = options
                    .OutputFormatters
                    .OfType<SystemTextJsonOutputFormatter>()
                    .First()
                    .SupportedMediaTypes;

                // Remove JSON text (text/json) media type from the system JSON input and output formatters.
                jsonSystemInputFormatterMediaTypes.Remove("text/json");
                jsonSystemOutputFormatterMediaTypes.Remove("text/json");

                // Add Problem Details media type (application/problem+json) to the JSON output formatters.
                // See https://tools.ietf.org/html/rfc7807
                jsonSystemOutputFormatterMediaTypes.Insert(0, ContentType.ProblemJson);
                ////xmlOutputFormatterMediaTypes.Insert(0, ContentType.ProblemXml);

                // Add RESTful JSON media type (application/vnd.restful+json) to the JSON input and output formatters.
                // See http://restfuljson.org/
                jsonSystemInputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);
                jsonSystemOutputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);


                // Add support for Newtonsoft JSON Patch (application/json-patch+json), configure formatters and make Newtonsoft default.
                options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
                options.InputFormatters.Insert(0, GetJsonInputFormatter());
                options.OutputFormatters.Insert(0, GetJsonOutputFormatter());

                MediaTypeCollection jsonNewtonsoftInputFormatterMediaTypes = options
                        .InputFormatters
                        .OfType<NewtonsoftJsonInputFormatter>()
                        .First()
                        .SupportedMediaTypes;
                MediaTypeCollection jsonNewtonsoftOutputFormatterMediaTypes = options
                    .OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()
                    .First()
                    .SupportedMediaTypes;

                jsonNewtonsoftInputFormatterMediaTypes.Remove("text/json");
                jsonNewtonsoftOutputFormatterMediaTypes.Remove("text/json");

                jsonNewtonsoftOutputFormatterMediaTypes.Insert(0, ContentType.ProblemJson);
                jsonNewtonsoftInputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);
                jsonNewtonsoftOutputFormatterMediaTypes.Insert(0, ContentType.RestfulJson);

                // Returns a 406 Not Acceptable if the MIME type in the Accept HTTP header is not valid.
                options.ReturnHttpNotAcceptable = true;
            });
        }

        /// <summary>
        /// Gets the JSON patch input formatter. The <see cref="JsonPatchDocument{T}"/> does not support the new
        /// System.Text.Json API's for de-serialization. You must use Newtonsoft.Json instead. See
        /// https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.0#jsonpatch-addnewtonsoftjson-and-systemtextjson
        /// </summary>
        /// <returns>The JSON patch input formatter using Newtonsoft.Json.</returns>
        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            IServiceCollection services = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                })
                .Services;
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            MvcOptions mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;
            return mvcOptions.InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }

        private static NewtonsoftJsonInputFormatter GetJsonInputFormatter()
        {
            IServiceCollection services = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                })
                .Services;
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            MvcOptions mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;
            return mvcOptions.InputFormatters
                .OfType<NewtonsoftJsonInputFormatter>()
                .Last();
        }

        private static NewtonsoftJsonOutputFormatter GetJsonOutputFormatter()
        {
            IServiceCollection services = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                })
                .Services;
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            MvcOptions mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;
            return mvcOptions.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()
                .Last();
        }

        public static IMvcBuilder AddCustomModelValidation(this IMvcBuilder builder)
        {
            return builder
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<Startup>();
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    fv.ImplicitlyValidateChildProperties = true;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        IRuntimeContextAccessor runtimeCtx = context.HttpContext.RequestServices.GetRequiredService<IRuntimeContextAccessor>();

                        ValidationProblemDetails problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Title = "Произошла ошибка валидации данных модели.",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "Обратитесь к свойству errors за дополнительной информацией.",
                            Instance = context.HttpContext.Request.Path
                        };
                        problemDetails.Extensions.Add("traceId", runtimeCtx.GetTraceId());
                        problemDetails.Extensions.Add("correlationId", runtimeCtx.GetCorrelationId());

                        return new BadRequestObjectResult(problemDetails);
                    };
                });
        }
    }
}
