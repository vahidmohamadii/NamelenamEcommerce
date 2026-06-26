using System.Net;
using System.Net.Http.Json;
using NaderEcommerce.Application.Cms;

namespace NaderEcommerce.BlazorWeb.Services;

public sealed class CmsApiClient(HttpClient httpClient)
{
    public async Task<CmsWebsiteSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CmsWebsiteSettingsDto>("api/cms/settings", cancellationToken);
    }

    public async Task<IReadOnlyList<CmsSliderDto>> GetSlidersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<CmsSliderDto>>("api/cms/sliders", cancellationToken)
            ?? Array.Empty<CmsSliderDto>();
    }

    public async Task<CmsPageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/cms/pages/by-slug/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CmsPageDto>(cancellationToken);
    }

    public async Task<CmsPageDto?> GetPageByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/cms/pages/by-key/{Uri.EscapeDataString(key)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CmsPageDto>(cancellationToken);
    }
}
