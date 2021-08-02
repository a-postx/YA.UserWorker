using System;
using System.Threading;
using System.Threading.Tasks;
using Delobytes.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using YA.Common.Constants;
using YA.UserWorker.Application.ActionHandlers.Users;
using YA.UserWorker.Application.Middlewares.ResourceFilters;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;

namespace YA.UserWorker.Controllers
{
    /// <summary>
    /// Обрабатывает запросы при работе с пользователями.
    /// </summary>
    [Route("/me")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Получить заголовок Allow с доступными методами для текущего пользователя.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("", Name = RouteNames.OptionsUser)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден.")]
        public IActionResult OptionsUser()
        {
            HttpContext.Response.Headers.AppendCommaSeparatedValues(
                HeaderNames.Allow,
                HttpMethods.Get,
                HttpMethods.Head,
                HttpMethods.Options,
                HttpMethods.Patch,
                HttpMethods.Post);
            return Ok();
        }

        /// <summary>
        /// Получить пользователя с арендаторами для текущего токена доступа.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>200 OK содержащий пользователя,
        /// 404 Не Найдено если пользователь не был найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpGet("", Name = RouteNames.GetUser)]
        [HttpHead("", Name = RouteNames.HeadUser)]
        [SwaggerResponse(StatusCodes.Status200OK, "Текущий пользователь.", typeof(UserVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "Пользователь не изменён с даты в заголовке If-Modified-Since.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> GetUserAsync(
            [FromServices] IGetUserAh handler,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Создать пользователя с необходимыми сущностями для текущего токена доступа.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="registrationInfo">Регистрационная информация.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК содержащий пользователя,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 409 Конфликт если запрос является дубликатом
        /// или 422 Неперевариваемая Сущность если такой пользователь уже существует.</returns>
        [HttpPost("", Name = RouteNames.PostUser)]
        [ServiceFilter(typeof(IdempotencyFilterAttribute))]
        [SwaggerResponse(StatusCodes.Status201Created, "Модель созданного пользователя.", typeof(UserVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]        
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Пользователь не может быть создан, поскольку уже существует.", typeof(ProblemDetails))]
        public Task<IActionResult> RegisterUserAsync(
            [FromServices] IRegisterNewUserAh handler,
            [FromBody] UserRegistrationInfoSm registrationInfo,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(registrationInfo, cancellationToken);
        }

        /// <summary>
        /// Переключить арендатора текущему пользователю.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="targetTenantId">Идентификатор арендатора, на который необходимо переключиться.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 ОК,
        /// 400 Недопустимый Запрос если запрос неправильно оформлен,
        /// 404 Не Найден если пользователь или целевой арендатор не найдены,
        /// 409 Конфликт если запрос является дубликатом</returns>
        [HttpPost("switchtenant", Name = RouteNames.SwitchUserTenant)]
        [Authorize(Policy = YaPolicyNames.NonAnonymous)]
        [ServiceFilter(typeof(IdempotencyFilterAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Арендатор переключен.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор или пользователь не найдены.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Недопустимый запрос.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> SwitchUserTenantAsync(
            [FromServices] ISwitchUserTenantAh handler,
            [FromQuery] Guid targetTenantId,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(targetTenantId, cancellationToken);
        }

        /// <summary>
        /// Обновить текущего пользователя.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="patch">Патч-документ. См. http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 OK если пользователь был изменён,
        /// 400 Недопустимый Запрос если параметры запроса неверны,
        /// 404 Не Найдено если текущий пользователь не был найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpPatch("", Name = RouteNames.PatchUser)]
        [ServiceFilter(typeof(IdempotencyFilterAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Модель изменённого пользователя.", typeof(UserVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Патч-документ неверен.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchUserAsync(
            [FromServices] IPatchUserAh handler,
            [FromBody] JsonPatchDocument<UserSm> patch,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(patch, cancellationToken);
        }
    }
}
