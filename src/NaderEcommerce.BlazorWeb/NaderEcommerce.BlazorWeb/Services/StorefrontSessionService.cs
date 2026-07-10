using System.Text.Json;
using Microsoft.JSInterop;
using NaderEcommerce.Application.Auth;

namespace NaderEcommerce.BlazorWeb.Services;

public sealed class StorefrontSessionService(IJSRuntime jsRuntime)
{
    private const string StorageKey = "nader.storefront.session";
    private const string AdminRole = "Admin";
    private bool isInitialized;

    public AuthResponse? Current { get; private set; }

    public bool IsAuthenticated =>
        Current is not null &&
        Current.AccessTokenExpiresAt > DateTimeOffset.UtcNow;

    public bool IsAdmin =>
        IsInRole(AdminRole);

    public event Action? Changed;

    public bool IsInRole(string role)
    {
        return IsAuthenticated
            && Current?.User.Roles.Any(currentRole => string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase)) == true;
    }

    public async Task EnsureInitializedAsync()
    {
        if (isInitialized)
        {
            return;
        }

        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("storefrontSession.get", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                Current = JsonSerializer.Deserialize<AuthResponse>(json);
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }

        isInitialized = true;
        Changed?.Invoke();
    }

    public async Task SignInAsync(AuthResponse response)
    {
        Current = response;
        isInitialized = true;
        await jsRuntime.InvokeVoidAsync("storefrontSession.set", StorageKey, JsonSerializer.Serialize(response));
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Current = null;
        isInitialized = true;

        try
        {
            await jsRuntime.InvokeVoidAsync("storefrontSession.remove", StorageKey);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }

        Changed?.Invoke();
    }
}
