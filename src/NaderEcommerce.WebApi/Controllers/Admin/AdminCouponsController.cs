using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Admin;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/coupons")]
public sealed class AdminCouponsController(
    IAdminService adminService,
    IValidator<UpsertCouponRequest> couponValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCouponDto>>> GetCoupons(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetCouponsAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AdminCouponDto>> CreateCoupon(
        UpsertCouponRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await couponValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.CreateCouponAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{couponId:guid}")]
    public async Task<ActionResult<AdminCouponDto>> UpdateCoupon(
        Guid couponId,
        UpsertCouponRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await couponValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.UpdateCouponAsync(couponId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{couponId:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid couponId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeleteCouponAsync(couponId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
