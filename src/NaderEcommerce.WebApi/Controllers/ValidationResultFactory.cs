using FluentValidation.Results;

namespace NaderEcommerce.WebApi.Controllers;

internal static class ValidationResultFactory
{
    public static object Create(ValidationResult validation)
    {
        return new
        {
            errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray())
        };
    }
}
