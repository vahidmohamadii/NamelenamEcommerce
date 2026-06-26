using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Catalog;

public sealed class QRLink : BaseEntity
{
    public string TargetUrl { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; } = new List<Product>();
}
