using NaderEcommerce.Application.Commerce;

namespace NaderEcommerce.Application.Admin;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<AdminCategoryDto> CreateCategoryAsync(UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task<AdminCategoryDto> UpdateCategoryAsync(Guid categoryId, UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminProductDto>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<AdminProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<AdminProductDto> CreateProductAsync(UpsertProductRequest request, CancellationToken cancellationToken = default);
    Task<AdminProductDto> UpdateProductAsync(Guid productId, UpsertProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminOrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderDetailsDto?> GetOrderAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<AdminOrderSummaryDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminCouponDto>> GetCouponsAsync(CancellationToken cancellationToken = default);
    Task<AdminCouponDto> CreateCouponAsync(UpsertCouponRequest request, CancellationToken cancellationToken = default);
    Task<AdminCouponDto> UpdateCouponAsync(Guid couponId, UpsertCouponRequest request, CancellationToken cancellationToken = default);
    Task DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminReviewDto>> GetReviewsAsync(CancellationToken cancellationToken = default);
    Task<AdminReviewDto> SetReviewApprovalAsync(Guid reviewId, SetReviewApprovalRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminSliderDto>> GetSlidersAsync(CancellationToken cancellationToken = default);
    Task<AdminSliderDto> CreateSliderAsync(UpsertSliderRequest request, CancellationToken cancellationToken = default);
    Task<AdminSliderDto> UpdateSliderAsync(Guid sliderId, UpsertSliderRequest request, CancellationToken cancellationToken = default);
    Task DeleteSliderAsync(Guid sliderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminPageDto>> GetPagesAsync(CancellationToken cancellationToken = default);
    Task<AdminPageDto> CreatePageAsync(UpsertPageRequest request, CancellationToken cancellationToken = default);
    Task<AdminPageDto> UpdatePageAsync(Guid pageId, UpsertPageRequest request, CancellationToken cancellationToken = default);
    Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken = default);

    Task<AdminWebsiteSettingsDto> GetWebsiteSettingsAsync(CancellationToken cancellationToken = default);
    Task<AdminWebsiteSettingsDto> UpdateWebsiteSettingsAsync(UpdateWebsiteSettingsRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminQrLinkDto>> GetQrLinksAsync(CancellationToken cancellationToken = default);
    Task<AdminQrLinkDto> CreateQrLinkAsync(UpsertQrLinkRequest request, CancellationToken cancellationToken = default);
    Task<AdminQrLinkDto> UpdateQrLinkAsync(Guid qrLinkId, UpsertQrLinkRequest request, CancellationToken cancellationToken = default);
    Task DeleteQrLinkAsync(Guid qrLinkId, CancellationToken cancellationToken = default);
}
