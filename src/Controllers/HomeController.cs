using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace YA.UserWorker.Controllers;

[Route("/")]
[ApiController]
[AllowAnonymous]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Корневая страница.
    /// </summary>
    /// <returns>Ответ 200 OK.
    /// </returns>
    [HttpGet("")]
    [SwaggerResponse(StatusCodes.Status200OK, "Пустой ответ 200.")]
    public IActionResult Get()
    {
        return Ok();
    }
}
