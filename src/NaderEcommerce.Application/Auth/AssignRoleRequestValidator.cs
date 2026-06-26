using FluentValidation;

namespace NaderEcommerce.Application.Auth;

public sealed class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    private static readonly string[] AllowedRoles = ["Admin", "Customer"];

    public AssignRoleRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.RoleName)
            .NotEmpty()
            .Must(role => AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("نقش باید ادمین یا مشتری باشد.");
    }
}
