using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Catalog;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Specifications { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public Guid? QrLinkId { get; set; }
    public QRLink? QrLink { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
    public ICollection<ProductImage> Images { get; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; } = new List<WishlistItem>();
}
