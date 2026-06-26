using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NaderEcommerce.Application.Admin;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.BlazorWeb.Services;

public sealed class AdminApiClient(HttpClient httpClient, StorefrontSessionService sessionService)
{
    public Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminDashboardDto>(HttpMethod.Get, "api/admin/dashboard", null, cancellationToken);

    public Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminCategoryDto>>(HttpMethod.Get, "api/admin/categories", null, cancellationToken);

    public Task<AdminCategoryDto> SaveCategoryAsync(Guid? id, UpsertCategoryRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminCategoryDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/categories" : $"api/admin/categories/{id}",
            request,
            cancellationToken);

    public Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/categories/{id}", null, cancellationToken);

    public Task<IReadOnlyList<AdminProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminProductDto>>(HttpMethod.Get, "api/admin/products", null, cancellationToken);

    public Task<AdminProductDto> SaveProductAsync(Guid? id, UpsertProductRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminProductDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/products" : $"api/admin/products/{id}",
            request,
            cancellationToken);

    public Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/products/{id}", null, cancellationToken);

    public Task<IReadOnlyList<AdminOrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminOrderSummaryDto>>(HttpMethod.Get, "api/admin/orders", null, cancellationToken);

    public Task<OrderDetailsDto> GetOrderAsync(string orderNumber, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<OrderDetailsDto>(HttpMethod.Get, $"api/admin/orders/{Uri.EscapeDataString(orderNumber)}", null, cancellationToken);

    public Task<AdminOrderSummaryDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminOrderSummaryDto>(HttpMethod.Patch, $"api/admin/orders/{orderId}/status", request, cancellationToken);

    public Task<IReadOnlyList<AdminCouponDto>> GetCouponsAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminCouponDto>>(HttpMethod.Get, "api/admin/coupons", null, cancellationToken);

    public Task<AdminCouponDto> SaveCouponAsync(Guid? id, UpsertCouponRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminCouponDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/coupons" : $"api/admin/coupons/{id}",
            request,
            cancellationToken);

    public Task DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/coupons/{id}", null, cancellationToken);

    public Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminUserDto>>(HttpMethod.Get, "api/admin/users", null, cancellationToken);

    public Task<AdminUserDto> SetUserActiveAsync(Guid userId, SetUserActiveRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminUserDto>(HttpMethod.Patch, $"api/admin/users/{userId}/active", request, cancellationToken);

    public Task<UserSummaryDto> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<UserSummaryDto>(HttpMethod.Post, "api/admin/users/roles", request, cancellationToken);

    public Task<IReadOnlyList<AdminReviewDto>> GetReviewsAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminReviewDto>>(HttpMethod.Get, "api/admin/reviews", null, cancellationToken);

    public Task<AdminReviewDto> SetReviewApprovalAsync(Guid reviewId, SetReviewApprovalRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminReviewDto>(HttpMethod.Patch, $"api/admin/reviews/{reviewId}/approval", request, cancellationToken);

    public Task<IReadOnlyList<AdminSliderDto>> GetSlidersAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminSliderDto>>(HttpMethod.Get, "api/admin/cms/sliders", null, cancellationToken);

    public Task<AdminSliderDto> SaveSliderAsync(Guid? id, UpsertSliderRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminSliderDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/cms/sliders" : $"api/admin/cms/sliders/{id}",
            request,
            cancellationToken);

    public Task DeleteSliderAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/cms/sliders/{id}", null, cancellationToken);

    public Task<IReadOnlyList<AdminPageDto>> GetPagesAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminPageDto>>(HttpMethod.Get, "api/admin/cms/pages", null, cancellationToken);

    public Task<AdminPageDto> SavePageAsync(Guid? id, UpsertPageRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminPageDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/cms/pages" : $"api/admin/cms/pages/{id}",
            request,
            cancellationToken);

    public Task DeletePageAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/cms/pages/{id}", null, cancellationToken);

    public Task<AdminWebsiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminWebsiteSettingsDto>(HttpMethod.Get, "api/admin/cms/settings", null, cancellationToken);

    public Task<AdminWebsiteSettingsDto> UpdateSettingsAsync(UpdateWebsiteSettingsRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminWebsiteSettingsDto>(HttpMethod.Put, "api/admin/cms/settings", request, cancellationToken);

    public Task<IReadOnlyList<AdminQrLinkDto>> GetQrLinksAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<AdminQrLinkDto>>(HttpMethod.Get, "api/admin/cms/qr-links", null, cancellationToken);

    public Task<AdminQrLinkDto> SaveQrLinkAsync(Guid? id, UpsertQrLinkRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<AdminQrLinkDto>(
            id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/admin/cms/qr-links" : $"api/admin/cms/qr-links/{id}",
            request,
            cancellationToken);

    public Task DeleteQrLinkAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAuthorizedNoContentAsync(HttpMethod.Delete, $"api/admin/cms/qr-links/{id}", null, cancellationToken);

    private async Task<T> SendAuthorizedAsync<T>(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        var response = await SendAuthorizedRequestAsync(method, uri, body, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new StorefrontApiException("پاسخ API خالی بود.", (int)response.StatusCode);
        }

        return result;
    }

    private async Task SendAuthorizedNoContentAsync(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        var response = await SendAuthorizedRequestAsync(method, uri, body, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAuthorizedRequestAsync(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        await sessionService.EnsureInitializedAsync();

        if (!sessionService.IsAuthenticated || sessionService.Current is null)
        {
            throw new StorefrontApiException("لطفا با حساب ادمین وارد شوید.", (int)HttpStatusCode.Unauthorized);
        }

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionService.Current.AccessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryExtractMessage(payload) ?? response.ReasonPhrase ?? "درخواست API مدیریت ناموفق بود.";
        throw new StorefrontApiException(message, (int)response.StatusCode);
    }

    private static string? TryExtractMessage(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }

            if (document.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var property in errors.EnumerateObject())
                {
                    var first = property.Value.EnumerateArray().FirstOrDefault().GetString();
                    if (!string.IsNullOrWhiteSpace(first))
                    {
                        return first;
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        return payload;
    }
}
