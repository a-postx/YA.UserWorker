using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IProblemDetailsFactory
    {
        ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null, string title = null, string type = null, string detail = null, string instance = null);
        ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationResult validationResult, int? statusCode = null, string title = null, string type = null, string detail = null, string instance = null);
    }
}
