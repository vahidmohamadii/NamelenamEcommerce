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

public sealed record CmsFaqItemDto(
    string Question,
    string Answer,
    int DisplayOrder);

public sealed record SubmitContactMessageRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    string Subject,
    string Message);

public sealed record SubmitContactMessageResponse(
    Guid Id,
    string Message);
