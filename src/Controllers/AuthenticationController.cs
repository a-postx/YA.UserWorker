using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.ActionFilters;
using YA.TenantWorker.Application.Commands;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [ServiceFilter(typeof(ApiRequestFilter))]
    public class AuthenticationController : Controller
    {
        /// <summary>
        /// Authenticate user and create a new token.
        /// </summary>
        /// <param name="command">Action command.</param>
        /// <param name="credentials">Credentials to use for authentication.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the HTTP request.</param>
        /// <returns>200 Created response containing newly created token,
        /// 400 Bad Request if the request is invalid
        /// or 409 Conflict if the request is a duplicate.</returns>
        [AllowAnonymous]
        [HttpPost("", Name = RouteNames.GetToken)]
        [SwaggerResponse(StatusCodes.Status200OK, "Token has been created.", typeof(TokenVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Request is invalid.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Duplicate request.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTokenAsync(
            [FromServices] IAuthenticateCommand command,
            [FromBody] CredentialsSm credentials,
            CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(credentials, cancellationToken);
        }
    }
}