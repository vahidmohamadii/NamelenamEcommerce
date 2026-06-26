namespace NaderEcommerce.BlazorWeb.Services;

public sealed class StorefrontApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
