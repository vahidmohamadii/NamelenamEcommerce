using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Admin;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/orders")]
public sealed class AdminOrdersController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminOrderSummaryDto>>> GetOrders(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetOrdersAsync(cancellationToken));
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrder(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await adminService.GetOrderAsync(orderNumber, cancellationToken);
        return order is null
            ? NotFound(new { message = "سفارش پیدا نشد." })
            : Ok(order);
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<ActionResult<AdminOrderSummaryDto>> UpdateOrderStatus(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await adminService.UpdateOrderStatusAsync(orderId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
