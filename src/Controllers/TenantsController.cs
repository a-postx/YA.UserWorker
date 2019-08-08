using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using YA.TenantWorker.ActionFilters;
using YA.TenantWorker.Commands;
using YA.TenantWorker.Constants;
using YA.TenantWorker.ViewModels;
using YA.TenantWorker.SaveModels;
using Swashbuckle.AspNetCore.Annotations;

namespace YA.TenantWorker.Controllers
{
    /// <summary>
    /// Control requests handling for tenant objects.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class TenantsController : ControllerBase
    {
        /// <summary>
        /// Return Allow HTTP header with allowed HTTP methods.
        /// </summary>
        /// <returns>200 OK response.</returns>
        [HttpOptions]
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
        /// Return Allow HTTP header with allowed HTTP methods for a tenant with the specified identifier.
        /// </summary>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <returns>200 OK response.</returns>
        [HttpOptions("{tenantId}")]
        [ServiceFilter(typeof(GetTenantRouteAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Allowed HTTP methods.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified identifier could not be found.")]
        public IActionResult Options(Guid tenantId)
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
        /// Get tenant with the specified identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK response containing the tenant or 404 Not Found if tenant
        /// with the specified unique identifier was not found.</returns>
        [HttpGet("{tenantId}", Name = RouteNames.GetTenant)]
        [HttpHead("{tenantId}", Name = RouteNames.HeadTenant)]
        [ServiceFilter(typeof(GetTenantRouteAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Tenant with the specified identifier.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "The tenant has not changed since the date given in the If-Modified-Since HTTP header.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified identifier could not be found.")]
        public Task<IActionResult> Get(
            [FromServices] IGetTenantCommand command,
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
        /// <returns>200 OK response containing a collection of tenants, 400 Bad Request if the page request
        /// parameters are invalid or 404 Not Found if a page with the specified page number was not found.
        /// </returns>
        [HttpGet("", Name = RouteNames.GetTenantPage)]
        [HttpHead("", Name = RouteNames.HeadTenantPage)]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection of tenants for the specified page.", typeof(PageResult<TenantVm>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Page request parameters are invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Page with the specified page number was not found.")]
        public Task<IActionResult> GetPage(
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
        /// <returns>201 Created response containing newly created tenant or 400 Bad Request if the tenant is
        /// invalid.</returns>
        [HttpPost("", Name = RouteNames.PostTenant)]
        [SwaggerResponse(StatusCodes.Status201Created, "Tenant was created.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Tenant is invalid.")]
        public Task<IActionResult> Post(
            [FromServices] IPostTenantCommand command,
            [FromBody] TenantSm tenantSm,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantSm, cancellationToken);
        }

        /// <summary>
        /// Patch tenant with the specified unique identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="patch">Patch document. See http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 OK if the tenant was patched, 400 Bad Request if the patch was invalid or 404 Not Found
        /// if a tenant with the specified unique identifier was not found.</returns>
        [HttpPatch("{tenantId}", Name = RouteNames.PatchTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Patched tenant with the specified unique identifier.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Patch document is invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified unique identifier could not be found.")]
        public Task<IActionResult> Patch(
            [FromServices] IPatchTenantCommand command,
            Guid tenantId,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, patch, cancellationToken);
        }

        /// <summary>
        /// Delete tenant with the specified unique identifier.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="tenantId">Tenant unique identifier.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>204 No Content response if the tenant was deleted or 404 Not Found if tenant with the specified
        /// unique identifier was not found.</returns>
        [HttpDelete("{tenantId}", Name = RouteNames.DeleteTenant)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Tenant with the specified unique identifier was deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant with the specified unique identifier was not found.")]
        public Task<IActionResult> Delete(
            [FromServices] IDeleteTenantCommand command,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(tenantId, cancellationToken);
        }
    }
}