namespace NaderEcommerce.Application.Auth;

public sealed record AssignRoleRequest(Guid UserId, string RoleName);
