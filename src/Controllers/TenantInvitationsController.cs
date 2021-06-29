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
using YA.UserWorker.Application.ActionHandlers.Invitations;
using YA.UserWorker.Application.Middlewares.ResourceFilters;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;

namespace YA.UserWorker.Controllers
{
    /// <summary>
    /// Обрабатывает запросы с приглашениями в арендатора.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [ServiceFilter(typeof(IdempotencyFilterAttribute))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class TenantInvitationsController : ControllerBase
    {
        /// <summary>
        /// Получить заголовок Allow с доступными методами для приглашений.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("", Name = RouteNames.OptionsTenantInvitations)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsInvitations()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Options,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Создать приглашение в текущий арендатор.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="inviteInfo">Модель приглашения.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК содержащий приглашение,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 409 Конфликт если запрос является дубликатом</returns>
        [HttpPost("", Name = RouteNames.PostTenantInvitation)]
        [Authorize(Policy = YaPolicyNames.Owner)]
        [SwaggerResponse(StatusCodes.Status201Created, "Модель созданного приглашения.", typeof(InvitationVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PostInvitationAsync(
            [FromServices] IPostInvitationAh handler,
            [FromBody] InvitationSm inviteInfo,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(inviteInfo, cancellationToken);
        }

        /// <summary>
        /// Получить заголовок Allow с доступными методами для приглашения.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("{invitationId}", Name = RouteNames.OptionsInvitation)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsInvitation()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Options,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Delete);
            return Ok();
        }

        /// <summary>
        /// Получить приглашение с указанным идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="invitationId">Идентификатор приглашения.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>200 OK содержащий модель приглашения,
        /// 404 Не Найдено если приглашение с таким идентификатором не найдено
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpGet("{invitationId}", Name = RouteNames.GetInvitation)]
        [HttpHead("{invitationId}", Name = RouteNames.HeadInvitation)]
        [SwaggerResponse(StatusCodes.Status200OK, "Приглашение с указанным идентификатором.", typeof(InvitationVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "Приглашение не изменено с даты в заголовке If-Modified-Since.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Приглашение не найдено.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> GetInvitationAsync(
            [FromServices] IGetInvitationAh handler,
            Guid invitationId,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(invitationId, cancellationToken);
        }

        /// <summary>
        /// Удалить приглашение с соответствующим идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="invitationId">Идентификатор приглашения.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 204 Без Содержимого если приглашение было удалено
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 404 Не Найден если приглашение не найдено,
        /// 409 Конфликт если запрос является дубликатом.</returns>
        [HttpDelete("{invitationId}", Name = RouteNames.DeleteInvitation)]
        [Authorize(Policy = YaPolicyNames.Owner)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Приглашение с указанным идентификатором было удалено.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Приглашение с указанным идентификатором не найдено.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> DeleteInvitationAsync(
            [FromServices] IDeleteInvitationAh handler,
            [FromRoute] Guid invitationId,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(invitationId, cancellationToken);
        }
    }
}
