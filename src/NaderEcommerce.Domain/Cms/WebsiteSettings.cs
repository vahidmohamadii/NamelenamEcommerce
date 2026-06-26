using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Cms;

public sealed class WebsiteSettings : BaseEntity
{
    public string SiteName { get; set; } = "NaderEcommerce";
    public string? LogoUrl { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? Address { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
}
