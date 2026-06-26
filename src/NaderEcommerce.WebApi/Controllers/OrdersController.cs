using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.WebApi.Controllers;

[Route("api/orders")]
public sealed class OrdersController(
    IOrderService orderService,
    IValidator<CheckoutRequest> checkoutValidator,
    IValidator<VerifyPaymentRequest> verifyPaymentValidator) : AuthorizedApiController
{
    [HttpGet("checkout")]
    public async Task<ActionResult<ShoppingCartDto>> GetCheckoutSummary(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await orderService.GetCheckoutSummaryAsync(userId.Value, cancellationToken));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutSessionDto>> Checkout(
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await checkoutValidator.ValidateAsync(request, cancellationToken);
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
            return Ok(await orderService.CheckoutAsync(userId.Value, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("payments/{paymentId:guid}/verify")]
    public async Task<ActionResult<OrderDetailsDto>> VerifyPayment(
        Guid paymentId,
        VerifyPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await verifyPaymentValidator.ValidateAsync(request, cancellationToken);
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
            return Ok(await orderService.VerifyPaymentAsync(userId.Value, paymentId, request, cancellationToken));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetOrders(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        return Ok(await orderService.GetOrdersAsync(userId.Value, cancellationToken));
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrder(string orderNumber, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "توکن دسترسی نامعتبر است." });
        }

        var order = await orderService.GetOrderAsync(userId.Value, orderNumber, cancellationToken);
        return order is null
            ? NotFound(new { message = "سفارش پیدا نشد." })
            : Ok(order);
    }
}
