using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Cms;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Cms;

public sealed class CmsService(ApplicationDbContext dbContext) : ICmsService
{
    public async Task<CmsWebsiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.WebsiteSettings
            .AsNoTracking()
            .OrderBy(entity => entity.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return settings is null
            ? new CmsWebsiteSettingsDto("NaderEcommerce", null, null, null, null, null, null)
            : new CmsWebsiteSettingsDto(
                settings.SiteName,
                settings.LogoUrl,
                settings.SupportEmail,
                settings.SupportPhone,
                settings.Address,
                settings.SeoTitle,
                settings.SeoDescription);
    }

    public async Task<IReadOnlyList<CmsSliderDto>> GetActiveSlidersAsync(CancellationToken cancellationToken = default)
    {
        var sliders = await dbContext.Sliders
            .AsNoTracking()
            .Where(slider => slider.IsActive)
            .OrderBy(slider => slider.DisplayOrder)
            .ThenBy(slider => slider.Title)
            .ToListAsync(cancellationToken);

        return sliders
            .Select(slider => new CmsSliderDto(
                slider.Title,
                slider.Subtitle,
                slider.ImageUrl,
                slider.LinkUrl,
                slider.DisplayOrder))
            .ToArray();
    }

    public async Task<CmsPageDto?> GetPublishedPageBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim();
        var page = await dbContext.Pages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entity => entity.IsPublished && entity.Slug == normalizedSlug,
                cancellationToken);

        return page is null ? null : MapPage(page);
    }

    public async Task<CmsPageDto?> GetPublishedPageByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim();
        var page = await dbContext.Pages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entity => entity.IsPublished && entity.Key == normalizedKey,
                cancellationToken);

        return page is null ? null : MapPage(page);
    }

    private static CmsPageDto MapPage(Domain.Cms.Page page)
    {
        return new CmsPageDto(
            page.Key,
            page.Title,
            page.Slug,
            page.Content,
            page.MetaTitle,
            page.MetaDescription);
    }
}
