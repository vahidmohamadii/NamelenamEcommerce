using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NaderEcommerce.Application.Auth;
using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.BlazorWeb.Services;

public sealed class StorefrontApiClient(HttpClient httpClient, StorefrontSessionService sessionService)
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        return await ReadRequiredAsync<AuthResponse>(response, cancellationToken);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
        return await ReadRequiredAsync<AuthResponse>(response, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await sessionService.EnsureInitializedAsync();
        if (sessionService.Current is null)
        {
            return;
        }

        var request = CreateAuthorizedRequest(HttpMethod.Post, "api/auth/logout");
        request.Content = JsonContent.Create(new RefreshTokenRequest(sessionService.Current.RefreshToken));
        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            await EnsureSuccessAsync(response, cancellationToken);
        }
    }

    public Task<ShoppingCartDto> GetCartAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Get, "api/cart", null, cancellationToken);

    public Task<ShoppingCartDto> AddCartItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Post, "api/cart/items", request, cancellationToken);

    public Task<ShoppingCartDto> UpdateCartItemAsync(Guid productId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Put, $"api/cart/items/{productId}", request, cancellationToken);

    public Task<ShoppingCartDto> RemoveCartItemAsync(Guid productId, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Delete, $"api/cart/items/{productId}", null, cancellationToken);

    public Task<ShoppingCartDto> ApplyCouponAsync(ApplyCouponRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Post, "api/cart/coupon", request, cancellationToken);

    public Task<ShoppingCartDto> RemoveCouponAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Delete, "api/cart/coupon", null, cancellationToken);

    public Task<IReadOnlyList<WishlistProductDto>> GetWishlistAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<WishlistProductDto>>(HttpMethod.Get, "api/wishlist", null, cancellationToken);

    public Task<IReadOnlyList<WishlistProductDto>> AddWishlistAsync(Guid productId, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<WishlistProductDto>>(HttpMethod.Post, $"api/wishlist/{productId}", null, cancellationToken);

    public Task<IReadOnlyList<WishlistProductDto>> RemoveWishlistAsync(Guid productId, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<WishlistProductDto>>(HttpMethod.Delete, $"api/wishlist/{productId}", null, cancellationToken);

    public Task<ShoppingCartDto> GetCheckoutSummaryAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<ShoppingCartDto>(HttpMethod.Get, "api/orders/checkout", null, cancellationToken);

    public Task<CheckoutSessionDto> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<CheckoutSessionDto>(HttpMethod.Post, "api/orders/checkout", request, cancellationToken);

    public Task<OrderDetailsDto> VerifyPaymentAsync(Guid paymentId, VerifyPaymentRequest request, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<OrderDetailsDto>(HttpMethod.Post, $"api/orders/payments/{paymentId}/verify", request, cancellationToken);

    public Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<IReadOnlyList<OrderSummaryDto>>(HttpMethod.Get, "api/orders", null, cancellationToken);

    public Task<OrderDetailsDto> GetOrderAsync(string orderNumber, CancellationToken cancellationToken = default)
        => SendAuthorizedAsync<OrderDetailsDto>(HttpMethod.Get, $"api/orders/{Uri.EscapeDataString(orderNumber)}", null, cancellationToken);

    private async Task<T> SendAuthorizedAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        await sessionService.EnsureInitializedAsync();

        var request = CreateAuthorizedRequest(method, uri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadRequiredAsync<T>(response, cancellationToken);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string uri)
    {
        if (!sessionService.IsAuthenticated || sessionService.Current is null)
        {
            throw new StorefrontApiException("برای ادامه ابتدا وارد حساب کاربری شوید.", (int)HttpStatusCode.Unauthorized);
        }

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionService.Current.AccessToken);
        return request;
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new StorefrontApiException("پاسخ API خالی بود.", (int)response.StatusCode);
        }

        return result;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryExtractMessage(payload) ?? response.ReasonPhrase ?? "درخواست API ناموفق بود.";
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
