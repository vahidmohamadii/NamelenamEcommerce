using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Authorize]
public abstract class AuthorizedApiController : ControllerBase
{
    protected Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
