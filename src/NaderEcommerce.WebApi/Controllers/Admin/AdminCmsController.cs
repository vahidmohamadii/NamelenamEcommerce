using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Admin;

namespace NaderEcommerce.WebApi.Controllers.Admin;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/cms")]
public sealed class AdminCmsController(
    IAdminService adminService,
    IValidator<UpsertSliderRequest> sliderValidator,
    IValidator<UpsertPageRequest> pageValidator,
    IValidator<UpdateWebsiteSettingsRequest> settingsValidator,
    IValidator<UpsertQrLinkRequest> qrLinkValidator) : ControllerBase
{
    [HttpGet("sliders")]
    public async Task<ActionResult<IReadOnlyList<AdminSliderDto>>> GetSliders(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetSlidersAsync(cancellationToken));
    }

    [HttpPost("sliders")]
    public async Task<ActionResult<AdminSliderDto>> CreateSlider(UpsertSliderRequest request, CancellationToken cancellationToken)
    {
        var validation = await sliderValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        return Ok(await adminService.CreateSliderAsync(request, cancellationToken));
    }

    [HttpPut("sliders/{sliderId:guid}")]
    public async Task<ActionResult<AdminSliderDto>> UpdateSlider(
        Guid sliderId,
        UpsertSliderRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await sliderValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.UpdateSliderAsync(sliderId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete("sliders/{sliderId:guid}")]
    public async Task<IActionResult> DeleteSlider(Guid sliderId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeleteSliderAsync(sliderId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("pages")]
    public async Task<ActionResult<IReadOnlyList<AdminPageDto>>> GetPages(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetPagesAsync(cancellationToken));
    }

    [HttpPost("pages")]
    public async Task<ActionResult<AdminPageDto>> CreatePage(UpsertPageRequest request, CancellationToken cancellationToken)
    {
        var validation = await pageValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.CreatePageAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("pages/{pageId:guid}")]
    public async Task<ActionResult<AdminPageDto>> UpdatePage(
        Guid pageId,
        UpsertPageRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await pageValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.UpdatePageAsync(pageId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("pages/{pageId:guid}")]
    public async Task<IActionResult> DeletePage(Guid pageId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeletePageAsync(pageId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("settings")]
    public async Task<ActionResult<AdminWebsiteSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetWebsiteSettingsAsync(cancellationToken));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<AdminWebsiteSettingsDto>> UpdateSettings(
        UpdateWebsiteSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await settingsValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        return Ok(await adminService.UpdateWebsiteSettingsAsync(request, cancellationToken));
    }

    [HttpGet("qr-links")]
    public async Task<ActionResult<IReadOnlyList<AdminQrLinkDto>>> GetQrLinks(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetQrLinksAsync(cancellationToken));
    }

    [HttpPost("qr-links")]
    public async Task<ActionResult<AdminQrLinkDto>> CreateQrLink(UpsertQrLinkRequest request, CancellationToken cancellationToken)
    {
        var validation = await qrLinkValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        return Ok(await adminService.CreateQrLinkAsync(request, cancellationToken));
    }

    [HttpPut("qr-links/{qrLinkId:guid}")]
    public async Task<ActionResult<AdminQrLinkDto>> UpdateQrLink(
        Guid qrLinkId,
        UpsertQrLinkRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await qrLinkValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationResultFactory.Create(validation));
        }

        try
        {
            return Ok(await adminService.UpdateQrLinkAsync(qrLinkId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete("qr-links/{qrLinkId:guid}")]
    public async Task<IActionResult> DeleteQrLink(Guid qrLinkId, CancellationToken cancellationToken)
    {
        try
        {
            await adminService.DeleteQrLinkAsync(qrLinkId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
