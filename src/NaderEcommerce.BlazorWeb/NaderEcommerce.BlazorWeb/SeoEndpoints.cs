using System.Security;
using System.Text;
using NaderEcommerce.Application.Catalog;
using NaderEcommerce.Application.Cms;
using NaderEcommerce.BlazorWeb.Services;

namespace NaderEcommerce.BlazorWeb;

public static class SeoEndpoints
{
    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/robots.txt", (HttpContext context) =>
        {
            var baseUrl = GetBaseUrl(context);
            var content = string.Join('\n',
                "User-agent: *",
                "Allow: /",
                "Disallow: /admin",
                "Disallow: /signin",
                "Disallow: /register",
                "Disallow: /cart",
                "Disallow: /checkout",
                "Disallow: /orders",
                "Disallow: /wishlist",
                $"Sitemap: {baseUrl}/sitemap.xml",
                string.Empty);

            return Results.Text(content, "text/plain; charset=utf-8");
        });

        endpoints.MapGet(
            "/sitemap.xml",
            async (HttpContext context, CatalogApiClient catalogApi, CmsApiClient cmsApi, CancellationToken cancellationToken) =>
            {
                var baseUrl = GetBaseUrl(context);
                var urls = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    $"{baseUrl}/",
                    $"{baseUrl}/products",
                    $"{baseUrl}/categories"
                };

                try
                {
                    var categories = await catalogApi.GetCategoriesAsync(cancellationToken);
                    foreach (var category in Flatten(categories))
                    {
                        urls.Add($"{baseUrl}/products?categorySlug={Uri.EscapeDataString(category.Slug)}");
                    }

                    var pageNumber = 1;
                    PagedResult<ProductCardDto>? page;
                    do
                    {
                        page = await catalogApi.GetProductsAsync(new ProductCatalogQuery
                        {
                            PageNumber = pageNumber,
                            PageSize = 48
                        }, cancellationToken);

                        if (page is null)
                        {
                            break;
                        }

                        foreach (var product in page.Items)
                        {
                            urls.Add($"{baseUrl}/products/{Uri.EscapeDataString(product.Slug)}");
                        }

                        pageNumber++;
                    }
                    while (page.HasNextPage && pageNumber <= 25);

                    await AddCmsPageAsync(urls, baseUrl, cmsApi, "about-us", cancellationToken);
                    await AddCmsPageAsync(urls, baseUrl, cmsApi, "contact-us", cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // Keep the sitemap available even if the API is temporarily unavailable.
                }

                var xml = BuildSitemap(urls);
                return Results.Text(xml, "application/xml; charset=utf-8");
            });

        return endpoints;
    }

    private static async Task AddCmsPageAsync(
        ISet<string> urls,
        string baseUrl,
        CmsApiClient cmsApi,
        string key,
        CancellationToken cancellationToken)
    {
        var page = await cmsApi.GetPageByKeyAsync(key, cancellationToken);
        if (page is not null)
        {
            urls.Add($"{baseUrl}/{Uri.EscapeDataString(page.Slug)}");
        }
    }

    private static IEnumerable<CategoryDto> Flatten(IEnumerable<CategoryDto> categories)
    {
        foreach (var category in categories)
        {
            yield return category;

            foreach (var child in Flatten(category.Children))
            {
                yield return child;
            }
        }
    }

    private static string BuildSitemap(IEnumerable<string> urls)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        foreach (var url in urls)
        {
            builder.AppendLine("  <url>");
            builder.Append("    <loc>")
                .Append(SecurityElement.Escape(url))
                .AppendLine("</loc>");
            builder.AppendLine("  </url>");
        }

        builder.AppendLine("</urlset>");
        return builder.ToString();
    }

    private static string GetBaseUrl(HttpContext context)
    {
        var configured = context.RequestServices
            .GetRequiredService<IConfiguration>()["Site:BaseUrl"];

        return string.IsNullOrWhiteSpace(configured)
            ? $"{context.Request.Scheme}://{context.Request.Host}".TrimEnd('/')
            : configured.TrimEnd('/');
    }
}
