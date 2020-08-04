using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.ActionFilters;
using YA.TenantWorker.Application.Commands;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using Microsoft.AspNetCore.Authorization;
using Delobytes.AspNetCore.Filters;

namespace YA.TenantWorker.Controllers
{
    /// <summary>
    /// Control requests handling for tenant objects.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [ServiceFilter(typeof(ApiRequestFilter))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class TenantsController : ControllerBase
    {
        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods for all tenants.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions("all", Name = RouteNames.OptionsTenantAll)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        public IActionResult OptionsAll()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Options);
            return Ok();
        }

        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods for tenants by id.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions("{tenantId}", Name = RouteNames.OptionsTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        public IActionResult OptionsById()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Get,
                HttpMethods.Options,
                HttpMethods.Patch,
                HttpMethods.Delete);
            return Ok();
        }

        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods for current tenant.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions("", Name = RouteNames.OptionsTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant could not be found.")]
        public IActionResult OptionsTenant()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Options,
                HttpMethods.Patch,
                HttpMethods.Delete);
            return Ok();
        }

        /// <summary>
        /// Get current tenant with pricing tier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK response containing the tenant,
        /// 404 Not Found if the current tenant was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpGet("", Name = RouteNames.GetTenant)]
        [HttpHead("", Name = RouteNames.HeadTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Current tenant.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "The tenant has not changed since the date given in the If-Modified-Since HTTP header.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Current tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantAsync([FromServices] IGetTenantCommand command, CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Get tenant with the specified unique identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK response containing the tenant,
        /// 404 Not Found if tenant with the specified unique identifier was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpGet("{tenantId}", Name = RouteNames.GetTenantById)]
        [HttpHead("{tenantId}", Name = RouteNames.HeadTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Current tenant.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "The tenant has not changed since the date given in the If-Modified-Since HTTP header.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Current tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantByIdAsync([FromServices] IGetTenantByIdCommand command,
            Guid tenantId, 
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, cancellationToken);
        }

        /// <summary>
        /// Get a collection of tenants using the specified paging options.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="pageOptions">Page options.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK response containing a collection of tenants, 400 Bad Request if the page request parameters are invalid,
        /// 404 Not Found if a page with the specified page number was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpGet("all", Name = RouteNames.GetTenantPage)]
        [HttpHead("all", Name = RouteNames.HeadTenantPage)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection of tenants for the specified page.", typeof(PaginatedResult<TenantVm>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Page request parameters are invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Page with the specified page number was not found.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantAllPageAsync(
            [FromServices] IGetTenantAllPageCommand command,
            [FromQuery] PageOptions pageOptions,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(pageOptions, cancellationToken);
        }

        /// <summary>
        /// Создать арендатора для нового неизвестного пользователя.
        /// </summary>
        /// <param name="command">Команда.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК содержащий ИД арендатора,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 409 Конфликт если запрос является дубликатом
        /// или 422 Неперевариваемая Сущность если такой арендатор уже существует.</returns>
        [HttpPost("", Name = RouteNames.PostTenant)]
        [SwaggerResponse(StatusCodes.Status201Created, "Модель созданного арендатора.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]        
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Арендатор не может быть создан, поскольку уже существует.", typeof(ProblemDetails))]
        public Task<IActionResult> PostTenantAsync(
            [FromServices] IPostTenantCommand command,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Update current tenant.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="patch">Patch document. See http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK if the tenant was patched, 400 Bad Request if the patch was invalid,
        /// 404 Not Found if the tenant was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpPatch("", Name = RouteNames.PatchTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Updated current tenant.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Patch document is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "The MIME type in the Content-Type HTTP header is unsupported.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchTenantAsync(
            [FromServices] IPatchTenantCommand command,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(patch, cancellationToken);
        }

        /// <summary>
        /// Update tenant with the specified unique identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="patch">Patch document. See http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK if the tenant was patched, 400 Bad Request if the patch was invalid,
        /// 404 Not Found if the tenant was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpPatch("{tenantId}", Name = RouteNames.PatchTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Tenant patched.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Patch document is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "The MIME type in the Content-Type HTTP header is unsupported.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchTenantByIdAsync(
            [FromServices] IPatchTenantByIdCommand command,
            Guid tenantId,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, patch, cancellationToken);
        }

        /// <summary>
        /// Delete current tenant.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>204 No Content response if the tenant was deleted
        /// 404 Not Found if the tenant was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpDelete("", Name = RouteNames.DeleteTenant)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Tenant was deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant was not found.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> DeleteTenantAsync(
            [FromServices] IDeleteTenantCommand command,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Delete tenant with the specified unique identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>204 No Content response if the tenant was deleted
        /// 404 Not Found if tenant with the specified unique identifier was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpDelete("{tenantId}", Name = RouteNames.DeleteTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Tenant with the specified unique identifier was deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified unique identifier was not found.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> DeleteTenantByIdAsync(
            [FromServices] IDeleteTenantByIdCommand command,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, cancellationToken);
        }
    }
}