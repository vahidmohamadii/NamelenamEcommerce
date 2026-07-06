using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace NaderEcommerce.BlazorWeb;

public static class SeoDefaults
{
    public const string SiteName = "فروشگاه نفس";
    public const string LatinBrandName = "nafasshop";
    public const string SiteUrl = "https://www.nafashshop786.com";
    public const string HomeTitle = "فروشگاه نفس | لوازم آرایشی و بهداشتی نفس";
    public const string HomeDescription =
        "فروشگاه نفس مرجع خرید لوازم آرایشی نفس و لوازم ارایشی و بهداشتی نفس با محصولات مراقبت پوست، آرایش، عطر و ابزار زیبایی. nafasshop و nafashshop786";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string PageTitle(string title)
    {
        return $"{title} | {SiteName}";
    }

    public static string AbsoluteUrl(NavigationManager navigation, string relativeUrl)
    {
        return navigation.ToAbsoluteUri(relativeUrl.TrimStart('/')).ToString();
    }

    public static string AbsoluteAssetUrl(NavigationManager navigation, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri)
            ? absoluteUri.ToString()
            : AbsoluteUrl(navigation, url);
    }

    public static string HomeStructuredData(string homeUrl, string productsUrl)
    {
        var data = new object[]
        {
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Organization",
                ["name"] = SiteName,
                ["alternateName"] = LatinBrandName,
                ["url"] = homeUrl
            },
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "WebSite",
                ["name"] = SiteName,
                ["alternateName"] = LatinBrandName,
                ["url"] = homeUrl,
                ["potentialAction"] = new Dictionary<string, object?>
                {
                    ["@type"] = "SearchAction",
                    ["target"] = $"{productsUrl}?q={{search_term_string}}",
                    ["query-input"] = "required name=search_term_string"
                }
            }
        };

        return JsonSerializer.Serialize(data, JsonOptions);
    }

    public static string ProductStructuredData(
        string name,
        string sku,
        string description,
        string canonicalUrl,
        string? imageUrl,
        decimal price,
        bool inStock)
    {
        var data = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = name,
            ["sku"] = sku,
            ["description"] = description,
            ["image"] = string.IsNullOrWhiteSpace(imageUrl) ? null : new[] { imageUrl },
            ["brand"] = new Dictionary<string, object?>
            {
                ["@type"] = "Brand",
                ["name"] = SiteName
            },
            ["offers"] = new Dictionary<string, object?>
            {
                ["@type"] = "Offer",
                ["url"] = canonicalUrl,
                ["priceCurrency"] = "IRR",
                ["price"] = price,
                ["availability"] = inStock
                    ? "https://schema.org/InStock"
                    : "https://schema.org/OutOfStock"
            }
        };

        return JsonSerializer.Serialize(data, JsonOptions);
    }
}
