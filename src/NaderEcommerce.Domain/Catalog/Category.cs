using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Catalog;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Category> Children { get; } = new List<Category>();
    public ICollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
}
