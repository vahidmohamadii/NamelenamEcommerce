using System.Globalization;
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
                $"Sitemap: {baseUrl}/sitemap.xml",
                string.Empty);

            return Results.Text(content, "text/plain; charset=utf-8");
        });

        endpoints.MapGet(
            "/sitemap.xml",
            async (HttpContext context, CatalogApiClient catalogApi, CmsApiClient cmsApi, CancellationToken cancellationToken) =>
            {
                var baseUrl = GetBaseUrl(context);
                var urls = new SortedDictionary<string, SitemapUrl>(StringComparer.OrdinalIgnoreCase);

                AddSitemapUrl(urls, $"{baseUrl}/", "daily", 1.0m);
                AddSitemapUrl(urls, $"{baseUrl}/products", "daily", 0.9m);
                AddSitemapUrl(urls, $"{baseUrl}/categories", "weekly", 0.8m);

                try
                {
                    var categories = await catalogApi.GetCategoriesAsync(cancellationToken);
                    foreach (var category in Flatten(categories))
                    {
                        AddSitemapUrl(
                            urls,
                            $"{baseUrl}/products?categorySlug={Uri.EscapeDataString(category.Slug)}",
                            "weekly",
                            0.7m);
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
                            AddSitemapUrl(
                                urls,
                                $"{baseUrl}/products/{Uri.EscapeDataString(product.Slug)}",
                                "weekly",
                                0.7m);
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

                var xml = BuildSitemap(urls.Values);
                return Results.Text(xml, "application/xml; charset=utf-8");
            });

        return endpoints;
    }

    private static async Task AddCmsPageAsync(
        IDictionary<string, SitemapUrl> urls,
        string baseUrl,
        CmsApiClient cmsApi,
        string key,
        CancellationToken cancellationToken)
    {
        var page = await cmsApi.GetPageByKeyAsync(key, cancellationToken);
        if (page is not null)
        {
            AddSitemapUrl(urls, $"{baseUrl}/{Uri.EscapeDataString(page.Slug)}", "monthly", 0.5m);
        }
    }

    private static void AddSitemapUrl(
        IDictionary<string, SitemapUrl> urls,
        string url,
        string changeFrequency,
        decimal priority)
    {
        urls[url] = new SitemapUrl(url, DateTimeOffset.UtcNow, changeFrequency, priority);
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

    private static string BuildSitemap(IEnumerable<SitemapUrl> urls)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        foreach (var url in urls)
        {
            builder.AppendLine("  <url>");
            builder.Append("    <loc>")
                .Append(SecurityElement.Escape(url.Location))
                .AppendLine("</loc>");
            builder.Append("    <lastmod>")
                .Append(url.LastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .AppendLine("</lastmod>");
            builder.Append("    <changefreq>")
                .Append(url.ChangeFrequency)
                .AppendLine("</changefreq>");
            builder.Append("    <priority>")
                .Append(url.Priority.ToString("0.0", CultureInfo.InvariantCulture))
                .AppendLine("</priority>");
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

    private sealed record SitemapUrl(
        string Location,
        DateTimeOffset LastModified,
        string ChangeFrequency,
        decimal Priority);
}
