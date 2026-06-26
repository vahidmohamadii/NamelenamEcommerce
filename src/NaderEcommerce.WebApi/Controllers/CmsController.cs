using Microsoft.AspNetCore.Mvc;
using NaderEcommerce.Application.Cms;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Route("api/cms")]
public sealed class CmsController(ICmsService cmsService) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<CmsWebsiteSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await cmsService.GetSettingsAsync(cancellationToken));
    }

    [HttpGet("sliders")]
    public async Task<ActionResult<IReadOnlyList<CmsSliderDto>>> GetSliders(CancellationToken cancellationToken)
    {
        return Ok(await cmsService.GetActiveSlidersAsync(cancellationToken));
    }

    [HttpGet("pages/by-slug/{slug}")]
    public async Task<ActionResult<CmsPageDto>> GetPageBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await cmsService.GetPublishedPageBySlugAsync(slug, cancellationToken);
        return page is null
            ? NotFound(new { message = "صفحه پیدا نشد." })
            : Ok(page);
    }

    [HttpGet("pages/by-key/{key}")]
    public async Task<ActionResult<CmsPageDto>> GetPageByKey(string key, CancellationToken cancellationToken)
    {
        var page = await cmsService.GetPublishedPageByKeyAsync(key, cancellationToken);
        return page is null
            ? NotFound(new { message = "صفحه پیدا نشد." })
            : Ok(page);
    }
}
