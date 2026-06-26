using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.WebApi.Controllers;

[Route("api/cart")]
public sealed class CartController(
    IShoppingCartService shoppingCartService,
    IValidator<AddCartItemRequest> addItemValidator,
    IValidator<UpdateCartItemRequest> updateItemValidator,
    IValidator<ApplyCouponRequest> applyCouponValidator) : AuthorizedApiController
{
    [HttpGet]
    public async Task<ActionResult<ShoppingCartDto>> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await shoppingCartService.GetCartAsync(userId.Value, cancellationToken));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ShoppingCartDto>> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var validation = await addItemValidator.ValidateAsync(request, cancellationToken);
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
            return Ok(await shoppingCartService.AddItemAsync(userId.Value, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<ShoppingCartDto>> UpdateItem(
        Guid productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await updateItemValidator.ValidateAsync(request, cancellationToken);
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
            return Ok(await shoppingCartService.UpdateItemAsync(userId.Value, productId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<ShoppingCartDto>> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await shoppingCartService.RemoveItemAsync(userId.Value, productId, cancellationToken));
    }

    [HttpPost("coupon")]
    public async Task<ActionResult<ShoppingCartDto>> ApplyCoupon(ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var validation = await applyCouponValidator.ValidateAsync(request, cancellationToken);
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
            return Ok(await shoppingCartService.ApplyCouponAsync(userId.Value, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("coupon")]
    public async Task<ActionResult<ShoppingCartDto>> RemoveCoupon(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await shoppingCartService.RemoveCouponAsync(userId.Value, cancellationToken));
    }
}
