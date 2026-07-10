using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NaderEcommerce.Application.Cms;
using System.Net.Mail;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Route("api/cms")]
public sealed class CmsController(ICmsService cmsService) : ControllerBase
{
    [HttpGet("settings")]
    [OutputCache(PolicyName = "PublicCms")]
    public async Task<ActionResult<CmsWebsiteSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await cmsService.GetSettingsAsync(cancellationToken));
    }

    [HttpGet("sliders")]
    [OutputCache(PolicyName = "PublicCms")]
    public async Task<ActionResult<IReadOnlyList<CmsSliderDto>>> GetSliders(CancellationToken cancellationToken)
    {
        return Ok(await cmsService.GetActiveSlidersAsync(cancellationToken));
    }

    [HttpGet("faqs")]
    [OutputCache(PolicyName = "PublicCms")]
    public async Task<ActionResult<IReadOnlyList<CmsFaqItemDto>>> GetFaqs(CancellationToken cancellationToken)
    {
        return Ok(await cmsService.GetActiveFaqsAsync(cancellationToken));
    }

    [HttpGet("pages/by-slug/{slug}")]
    [OutputCache(PolicyName = "PublicCms")]
    public async Task<ActionResult<CmsPageDto>> GetPageBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await cmsService.GetPublishedPageBySlugAsync(slug, cancellationToken);
        return page is null
            ? NotFound(new { message = "صفحه پیدا نشد." })
            : Ok(page);
    }

    [HttpGet("pages/by-key/{key}")]
    [OutputCache(PolicyName = "PublicCms")]
    public async Task<ActionResult<CmsPageDto>> GetPageByKey(string key, CancellationToken cancellationToken)
    {
        var page = await cmsService.GetPublishedPageByKeyAsync(key, cancellationToken);
        return page is null
            ? NotFound(new { message = "صفحه پیدا نشد." })
            : Ok(page);
    }

    [HttpPost("contact-messages")]
    public async Task<ActionResult<SubmitContactMessageResponse>> SubmitContactMessage(
        SubmitContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateContactMessage(request);
        if (validationErrors.Count > 0)
        {
            return BadRequest(new { errors = validationErrors });
        }

        return Ok(await cmsService.SubmitContactMessageAsync(request, cancellationToken));
    }

    private static Dictionary<string, string[]> ValidateContactMessage(SubmitContactMessageRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 160)
        {
            errors[nameof(request.FullName)] = ["نام را وارد کنید."];
        }

        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 256 || !IsValidEmail(request.Email))
        {
            errors[nameof(request.Email)] = ["ایمیل معتبر وارد کنید."];
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber.Length > 32)
        {
            errors[nameof(request.PhoneNumber)] = ["شماره تماس معتبر وارد کنید."];
        }

        if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Length > 220)
        {
            errors[nameof(request.Subject)] = ["موضوع را وارد کنید."];
        }

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 3000)
        {
            errors[nameof(request.Message)] = ["متن پیام را وارد کنید."];
        }

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
