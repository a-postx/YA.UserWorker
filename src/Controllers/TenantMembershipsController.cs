using System;
using System.Threading;
using System.Threading.Tasks;
using Delobytes.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using YA.Common.Constants;
using YA.UserWorker.Application.ActionHandlers.Memberships;
using YA.UserWorker.Application.Middlewares.ResourceFilters;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;

namespace YA.UserWorker.Controllers
{
    /// <summary>
    /// Обрабатывает запросы с членством в арендаторе.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [ServiceFilter(typeof(IdempotencyFilterAttribute))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class TenantMembershipsController : ControllerBase
    {
        /// <summary>
        /// Получить заголовок Allow с доступными методами для членств.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("", Name = RouteNames.OptionsTenantMemberships)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsTenantMemberships()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Options,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Создать членство в арендаторе.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="token">Идентификатор приглашения.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК содержащий членство,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 409 Конфликт если запрос является дубликатом</returns>
        [HttpPost("", Name = RouteNames.PostTenantMembership)]
        [SwaggerResponse(StatusCodes.Status201Created, "Модель созданного членства.", typeof(MembershipVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PostMembershipAsync(
            [FromServices] IPostMembershipAh handler,
            [FromQuery] Guid token,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(token, cancellationToken);
        }

        /// <summary>
        /// Получить заголовок Allow с доступными методами для членства.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("{membershipId}", Name = RouteNames.OptionsMembership)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsMembership()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Options,
                HttpMethods.Delete);
            return Ok();
        }

        /// <summary>
        /// Удалить членство с соответствующим идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="membershipId">Идентификатор членства.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 204 Без Содержимого если приглашение было удалено
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 404 Не Найден если приглашение не найдено,
        /// 409 Конфликт если запрос является дубликатом.</returns>
        [HttpDelete("{membershipId}", Name = RouteNames.DeleteMembership)]
        [Authorize(Policy = YaPolicyNames.Owner)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Членство с указанным идентификатором было удалено.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Членство с указанным идентификатором не найдено.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> DeleteMembershipAsync(
            [FromServices] IDeleteMembershipAh handler,
            [FromRoute] Guid membershipId,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(membershipId, cancellationToken);
        }
    }
}
