using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IValidationProblemDetailsGenerator
    {
        public ValidationProblemDetails Generate(ValidationResult validationResult);
        public ValidationProblemDetails Generate(ModelStateDictionary modelState);
    }
}
