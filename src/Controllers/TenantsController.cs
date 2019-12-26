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
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Controllers
{
    /// <summary>
    /// Control requests handling for tenant objects.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [ServiceFilter(typeof(ApiRequestFilter))]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, HttpCodeMessages.Code500ErrorMessage, typeof(ApiProblemDetails))]
    public class TenantsController : ControllerBase
    {
        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions(Name = RouteNames.OptionsTenants)]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Options,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods for current tenant.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions("tenant", Name = RouteNames.OptionsTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant could not be found.")]
        public IActionResult OptionsTenant()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Delete,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Options,
                HttpMethods.Patch,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Get current tenant.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK response containing the tenant,
        /// 404 Not Found if tenant with the specified unique identifier was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpGet("tenant", Name = RouteNames.GetTenant)]
        [HttpHead("tenant", Name = RouteNames.HeadTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Current tenant.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "The tenant has not changed since the date given in the If-Modified-Since HTTP header.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Current tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ApiProblemDetails))]
        public Task<IActionResult> GetTenantAsync([FromServices] IGetTenantCommand command, CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(cancellationToken);
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
        [HttpGet("", Name = RouteNames.GetTenantPage)]
        [HttpHead("", Name = RouteNames.HeadTenantPage)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection of tenants for the specified page.", typeof(PaginatedResult<TenantVm>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Page request parameters are invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Page with the specified page number was not found.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ApiProblemDetails))]
        public Task<IActionResult> GetTenantPageAsync(
            [FromServices] IGetTenantPageCommand command,
            [FromQuery] PageOptions pageOptions,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(pageOptions, cancellationToken);
        }

        /// <summary>
        /// Create a new tenant.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantSm">Tenant to create.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>201 Created response containing newly created tenant,
        /// 400 Bad Request if the request is invalid
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpPost("", Name = RouteNames.PostTenant)]
        [SwaggerResponse(StatusCodes.Status201Created, "Tenant was created.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Request is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ApiProblemDetails))]        
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "The MIME type in the Content-Type HTTP header is unsupported.", typeof(ProblemDetails))]
        public Task<IActionResult> PostTenantAsync(
            [FromServices] IPostTenantCommand command,
            [FromBody] TenantSm tenantSm,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantSm, cancellationToken);
        }

        /// <summary>
        /// Patch current tenant.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="patch">Patch document. See http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK if the tenant was patched, 400 Bad Request if the patch was invalid,
        /// 404 Not Found if tenant was not found
        /// or 409 Conflict if the request is a duplicate.</returns>
        [HttpPatch("tenant", Name = RouteNames.PatchTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Patched current tenant.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Patch document is invalid.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant could not be found.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "The MIME type in the Accept HTTP header is not acceptable.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ApiProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "The MIME type in the Content-Type HTTP header is unsupported.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchTenantAsync(
            [FromServices] IPatchTenantCommand command,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(patch, cancellationToken);
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
        [HttpDelete("{tenantId}", Name = RouteNames.DeleteTenant)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Tenant with the specified unique identifier was deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified unique identifier was not found.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ApiProblemDetails))]
        public Task<IActionResult> DeleteTenantAsync(
            [FromServices] IDeleteTenantCommand command,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, cancellationToken);
        }
    }
}