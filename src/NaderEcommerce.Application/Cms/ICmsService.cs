namespace NaderEcommerce.Application.Cms;

public interface ICmsService
{
    Task<CmsWebsiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CmsSliderDto>> GetActiveSlidersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CmsFaqItemDto>> GetActiveFaqsAsync(CancellationToken cancellationToken = default);
    Task<CmsPageDto?> GetPublishedPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CmsPageDto?> GetPublishedPageByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<SubmitContactMessageResponse> SubmitContactMessageAsync(
        SubmitContactMessageRequest request,
        CancellationToken cancellationToken = default);
}
