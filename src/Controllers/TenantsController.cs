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
using YA.UserWorker.Application.ActionHandlers.Tenants;
using YA.UserWorker.Application.Middlewares.ResourceFilters;
using YA.UserWorker.Application.Models.HttpQueryParams;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;

namespace YA.UserWorker.Controllers
{
    /// <summary>
    /// Обрабатывает запросы при работе с арендаторами.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion(ApiVersionName.V1)]
    [Authorize]
    [NoCache]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.Code500, typeof(ProblemDetails))]
    public class TenantsController : ControllerBase
    {
        /// <summary>
        /// Получить заголовок Allow с доступными методами для всех арендаторов.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("all", Name = RouteNames.OptionsTenantAll)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
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
        /// Получить заголовок Allow с доступными методами для арендатора с указанным идентификатором.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("{tenantId}", Name = RouteNames.OptionsTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        public IActionResult OptionsById()
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
        /// Получить заголовок Allow с доступными методами для текущего арендатора.
        /// </summary>
        /// <returns>Ответ 200 OK.</returns>
        [HttpOptions("", Name = RouteNames.OptionsTenant)]
        [SwaggerResponse(StatusCodes.Status200OK, "Доступные HTTP методы.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор не найден.")]
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
        /// Получить текущего арендатора с тарифным планом.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>200 OK содержащий арендатора,
        /// 404 Не Найдено если арендатор не был найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpGet("", Name = RouteNames.GetTenant)]
        [HttpHead("", Name = RouteNames.HeadTenant)]
        [Authorize(Policy = YaPolicyNames.Reader)]
        [SwaggerResponse(StatusCodes.Status200OK, "Текущий арендатор.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "Арендатор не изменён с даты в заголовке If-Modified-Since.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantAsync(
            [FromServices] IGetTenantAh handler,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Получить арендатора с указанным идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="tenantId">Идентификатор арендатора.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>200 OK содержащий модель арендатора,
        /// 404 Не Найдено если арендатор с таким идентификатором не найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpGet("{tenantId}", Name = RouteNames.GetTenantById)]
        [HttpHead("{tenantId}", Name = RouteNames.HeadTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Арендатор с указанным идентификатором.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status304NotModified, "Арендатор не изменён с даты в заголовке If-Modified-Since.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantByIdAsync(
            [FromServices] IGetTenantByIdAh handler,
            Guid tenantId, 
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(tenantId, cancellationToken);
        }

        /// <summary>
        /// Получить список арендаторов, используя указанные настройки постраничного вывода.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="pageOptions">Настройки постраничного вывода.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 OK содержащий список арендаторов,
        /// 400 Недопустимый Запрос если параметры запроса неверны,
        /// 404 Не Найдено если страница с указанным номером не была найдена
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpGet("all", Name = RouteNames.GetTenantPage)]
        [HttpHead("all", Name = RouteNames.HeadTenantPage)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список арендаторов на указанной странице.", typeof(PaginatedResultVm<TenantVm>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Параметры запроса неверны.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Страница с указанным номером не найдена.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> GetTenantAllPageAsync(
            [FromServices] IGetTenantAllPageAh handler,
            [FromQuery] PageOptionsCursor pageOptions,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(pageOptions, cancellationToken);
        }

        /// <summary>
        /// Обновить текущего арендатора.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="patch">Патч-документ. См. http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 OK если арендатор был изменён,
        /// 400 Недопустимый Запрос если параметры запроса неверны,
        /// 404 Не Найдено если текущий арендатор не был найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpPatch("", Name = RouteNames.PatchTenant)]
        [Authorize(Policy = YaPolicyNames.Admin)]
        [ServiceFilter(typeof(IdempotencyFilterAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Модель изменённого арендатора.", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Патч-документ неверен.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchTenantAsync(
            [FromServices] IPatchTenantAh handler,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(patch, cancellationToken);
        }

        /// <summary>
        /// Обновить арендатора с указанным идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="tenantId">Идентификатор арендатора.</param>
        /// <param name="patch">Патч-документ. См. http://jsonpatch.com.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 200 OK если арендатор был изменён,
        /// 400 Недопустимый Запрос если параметры запроса неверны,
        /// 404 Не Найдено если арендатор с указанным идентификатором не был найден
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpPatch("{tenantId}", Name = RouteNames.PatchTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [ServiceFilter(typeof(IdempotencyFilterAttribute))]
        [SwaggerResponse(StatusCodes.Status200OK, "Арендатор обновлён", typeof(TenantVm))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Патч-документ неверен.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор с указанным идентификатором не найден.")]
        [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Недопустимый тип MIME в заголовке Accept.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Тип MIME в заголовке Content-Type не поддерживается.", typeof(ProblemDetails))]
        public Task<IActionResult> PatchTenantByIdAsync(
            [FromServices] IPatchTenantByIdAh handler,
            Guid tenantId,
            [FromBody] JsonPatchDocument<TenantSm> patch,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(tenantId, patch, cancellationToken);
        }

        /// <summary>
        /// Удалить арендатора с указанным идентификатором.
        /// </summary>
        /// <param name="handler">Обработчик.</param>
        /// <param name="tenantId">Идентификатор арендатора.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Ответ 204 Без Содержимого если арендатор был удалён
        /// 404 Не Найден если арендатора с указанным идентификатором не было найдено
        /// или 409 Конфликт если запрос является дубликатом.</returns>
        [HttpDelete("{tenantId}", Name = RouteNames.DeleteTenantById)]
        [Authorize(Policy = "MustBeAdministrator")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Арендатор с указанным идентификатором был удалён.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Арендатор с указанным идентификатором не найден.")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Запрос-дубликат.", typeof(ProblemDetails))]
        public Task<IActionResult> DeleteTenantByIdAsync(
            [FromServices] IDeleteTenantByIdAh handler,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            return handler.ExecuteAsync(tenantId, cancellationToken);
        }
    }
}
