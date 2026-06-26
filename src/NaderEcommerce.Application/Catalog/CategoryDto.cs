namespace NaderEcommerce.Application.Catalog;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    int ProductCount,
    IReadOnlyList<CategoryDto> Children);
