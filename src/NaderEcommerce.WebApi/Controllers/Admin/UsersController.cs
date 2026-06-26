using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Auth;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/users")]
public sealed class UsersController(
    IUserRoleService userRoleService,
    IValidator<AssignRoleRequest> assignRoleValidator)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(
        CancellationToken cancellationToken)
    {
        return Ok(await userRoleService.GetUsersAsync(cancellationToken));
    }

    [HttpPost("roles")]
    public async Task<ActionResult<UserSummaryDto>> AssignRole(
        AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await assignRoleValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                errors = validation.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }

        try
        {
            return Ok(await userRoleService.AssignRoleAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPatch("{userId:guid}/active")]
    public async Task<ActionResult<AdminUserDto>> SetActive(
        Guid userId,
        SetUserActiveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await userRoleService.SetUserActiveAsync(userId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
