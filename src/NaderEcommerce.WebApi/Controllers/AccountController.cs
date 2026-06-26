using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Auth;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(
    IAccountService accountService,
    IValidator<UpdateProfileRequest> updateProfileValidator)
    : AuthorizedApiController
{
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        try
        {
            return Ok(await accountService.GetProfileAsync(userId.Value, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await updateProfileValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        try
        {
            return Ok(await accountService.UpdateProfileAsync(userId.Value, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
