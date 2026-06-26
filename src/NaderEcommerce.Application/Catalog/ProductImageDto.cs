namespace NaderEcommerce.Application.Catalog;

public sealed record ProductImageDto(
    string Url,
    string? AltText,
    bool IsPrimary,
    int DisplayOrder);
