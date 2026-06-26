using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.WebApi.Controllers;

[Route("api/wishlist")]
public sealed class WishlistController(IWishlistService wishlistService) : AuthorizedApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WishlistProductDto>>> GetWishlist(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await wishlistService.GetWishlistAsync(userId.Value, cancellationToken));
    }

    [HttpPost("{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<WishlistProductDto>>> Add(Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        try
        {
            return Ok(await wishlistService.AddAsync(userId.Value, productId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<WishlistProductDto>>> Remove(Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await wishlistService.RemoveAsync(userId.Value, productId, cancellationToken));
    }
}
