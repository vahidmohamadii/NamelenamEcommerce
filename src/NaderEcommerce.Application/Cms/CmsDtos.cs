namespace NaderEcommerce.Application.Cms;

public sealed record CmsWebsiteSettingsDto(
    string SiteName,
    string? LogoUrl,
    string? SupportEmail,
    string? SupportPhone,
    string? Address,
    string? SeoTitle,
    string? SeoDescription);

public sealed record CmsSliderDto(
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder);

public sealed record CmsPageDto(
    string Key,
    string Title,
    string Slug,
    string Content,
    string? MetaTitle,
    string? MetaDescription);
