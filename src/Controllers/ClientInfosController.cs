using Delobytes.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Middlewares.ResourceFilters;
using YA.TenantWorker.Application.ActionHandlers.ClientInfos;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Controllers
{
    /// <summary>
    /// Обрабатывает запросы с объектами информации о клиенте.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [ServiceFilter(typeof(IdempotencyFilterAttribute))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class ClientInfosController : ControllerBase
    {
        /// <summary>
        /// Получить заголовок Allow с доступными методами.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("", Name = RouteNames.OptionsClientInfo)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsClientInfo()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Options,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Создать событие публикации информации о клиенте.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="clientInfoSm">Информация о клиенте.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК содержащий результат публикации,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 409 Конфликт если запрос является дубликатом</returns>
        [HttpPost("", Name = RouteNames.PostClientInfo)]
        [SwaggerResponse(StatusCodes.Status201Created, "Модель события информации о клиенте.", typeof(ClientInfoVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]        
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PostClientInfoAsync(
            [FromServices] IPostClientInfoAh handler,
            [FromBody] ClientInfoSm clientInfoSm,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(clientInfoSm, cancellationToken);
        }
    }
}
