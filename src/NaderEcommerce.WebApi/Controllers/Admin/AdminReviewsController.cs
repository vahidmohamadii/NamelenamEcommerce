using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Admin;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/reviews")]
public sealed class AdminReviewsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminReviewDto>>> GetReviews(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetReviewsAsync(cancellationToken));
    }

    [HttpPatch("{reviewId:guid}/approval")]
    public async Task<ActionResult<AdminReviewDto>> SetReviewApproval(
        Guid reviewId,
        SetReviewApprovalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await adminService.SetReviewApprovalAsync(reviewId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
