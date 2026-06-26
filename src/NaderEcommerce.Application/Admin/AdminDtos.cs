using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Application.Admin;

public sealed record AdminDashboardDto(
    int ProductCount,
    int ActiveProductCount,
    int CategoryCount,
    int PendingOrderCount,
    int PaidOrderCount,
    int CustomerCount,
    int ReviewCount,
    int ActiveCouponCount,
    decimal RevenueTotal,
    IReadOnlyList<AdminOrderSummaryDto> RecentOrders);

public sealed record AdminCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    int SortOrder,
    int ProductCount,
    DateTimeOffset CreatedAt);

public sealed record UpsertCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    int SortOrder);

public sealed record AdminProductDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string? Description,
    string? Specifications,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    bool IsActive,
    bool IsFeatured,
    bool IsBestSeller,
    Guid? QrLinkId,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<AdminProductImageDto> Images,
    DateTimeOffset CreatedAt);

public sealed record AdminProductImageDto(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary);

public sealed record UpsertProductRequest(
    string Name,
    string Slug,
    string Sku,
    string? Description,
    string? Specifications,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    bool IsActive,
    bool IsFeatured,
    bool IsBestSeller,
    Guid? QrLinkId,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<UpsertProductImageRequest> Images);

public sealed record UpsertProductImageRequest(
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary);

public sealed record AdminOrderSummaryDto(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    string CustomerFullName,
    string CustomerEmail,
    decimal TotalAmount,
    int ItemCount,
    DateTimeOffset CreatedAt);

public sealed record UpdateOrderStatusRequest(OrderStatus Status);

public sealed record AdminCouponDto(
    Guid Id,
    string Code,
    decimal DiscountAmount,
    decimal? MinimumOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    int? UsageLimit,
    int UsedCount,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record UpsertCouponRequest(
    string Code,
    decimal DiscountAmount,
    decimal? MinimumOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    int? UsageLimit,
    bool IsActive);

public sealed record AdminReviewDto(
    Guid Id,
    string ProductName,
    string CustomerFullName,
    int Rating,
    string? Comment,
    bool IsApproved,
    DateTimeOffset CreatedAt);

public sealed record SetReviewApprovalRequest(bool IsApproved);

public sealed record AdminSliderDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive);

public sealed record UpsertSliderRequest(
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive);

public sealed record AdminPageDto(
    Guid Id,
    string Key,
    string Title,
    string Slug,
    string Content,
    string? MetaTitle,
    string? MetaDescription,
    bool IsPublished);

public sealed record UpsertPageRequest(
    string Key,
    string Title,
    string Slug,
    string Content,
    string? MetaTitle,
    string? MetaDescription,
    bool IsPublished);

public sealed record AdminWebsiteSettingsDto(
    Guid Id,
    string SiteName,
    string? LogoUrl,
    string? SupportEmail,
    string? SupportPhone,
    string? Address,
    string? SeoTitle,
    string? SeoDescription);

public sealed record UpdateWebsiteSettingsRequest(
    string SiteName,
    string? LogoUrl,
    string? SupportEmail,
    string? SupportPhone,
    string? Address,
    string? SeoTitle,
    string? SeoDescription);

public sealed record AdminQrLinkDto(
    Guid Id,
    string TargetUrl,
    string? Label,
    bool IsActive,
    int ProductCount);

public sealed record UpsertQrLinkRequest(
    string TargetUrl,
    string? Label,
    bool IsActive);
