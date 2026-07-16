using Microsoft.EntityFrameworkCore;
using NaderEcommerce.Application.Admin;
using NaderEcommerce.Application.Commerce;
using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Cms;
using NaderEcommerce.Domain.Identity;
using NaderEcommerce.Domain.Orders;
using NaderEcommerce.Infrastructure.Commerce;
using NaderEcommerce.Infrastructure.Persistence;

namespace NaderEcommerce.Infrastructure.Admin;

public sealed class AdminService(ApplicationDbContext dbContext) : IAdminService
{
    private const int MaxAdminListItems = 200;

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var recentOrders = await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        return new AdminDashboardDto(
            await dbContext.Products.CountAsync(cancellationToken),
            await dbContext.Products.CountAsync(product => product.IsActive, cancellationToken),
            await dbContext.Categories.CountAsync(cancellationToken),
            await dbContext.Orders.CountAsync(order => order.Status == OrderStatus.Pending, cancellationToken),
            await dbContext.Orders.CountAsync(order => order.Status == OrderStatus.Paid, cancellationToken),
            await dbContext.Users.CountAsync(user => user.UserRoles.Any(role => role.Role.Name == Role.Customer), cancellationToken),
            await dbContext.Reviews.CountAsync(cancellationToken),
            await dbContext.Coupons.CountAsync(coupon => coupon.IsActive, cancellationToken),
            await dbContext.Payments
                .Where(payment => payment.Status == PaymentStatus.Succeeded)
                .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m,
            recentOrders.Select(MapOrderSummary).ToArray());
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Include(category => category.ProductCategories)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(MapCategory).ToArray();
    }

    public async Task<AdminCategoryDto> CreateCategoryAsync(
        UpsertCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategorySlugIsUniqueAsync(request.Slug, null, cancellationToken);
        await EnsureParentCategoryExistsAsync(request.ParentCategoryId, null, cancellationToken);

        var category = new Category();
        ApplyCategory(category, request);
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapCategory(category);
    }

    public async Task<AdminCategoryDto> UpdateCategoryAsync(
        Guid categoryId,
        UpsertCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .Include(entity => entity.ProductCategories)
            .SingleOrDefaultAsync(entity => entity.Id == categoryId, cancellationToken)
            ?? throw new InvalidOperationException("دسته‌بندی پیدا نشد.");

        await EnsureCategorySlugIsUniqueAsync(request.Slug, categoryId, cancellationToken);
        await EnsureParentCategoryExistsAsync(request.ParentCategoryId, categoryId, cancellationToken);

        ApplyCategory(category, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapCategory(category);
    }

    public async Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .Include(entity => entity.Children)
            .Include(entity => entity.ProductCategories)
            .SingleOrDefaultAsync(entity => entity.Id == categoryId, cancellationToken)
            ?? throw new InvalidOperationException("دسته‌بندی پیدا نشد.");

        if (category.Children.Count > 0 || category.ProductCategories.Count > 0)
        {
            category.IsActive = false;
        }
        else
        {
            dbContext.Categories.Remove(category);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await ProductQuery()
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderByDescending(product => product.CreatedAt)
            .Take(MaxAdminListItems)
            .ToListAsync(cancellationToken);

        return products.Select(MapProduct).ToArray();
    }

    public async Task<AdminProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await ProductQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == productId, cancellationToken);

        return product is null ? null : MapProduct(product);
    }

    public async Task<AdminProductDto> CreateProductAsync(
        UpsertProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductKeysAreUniqueAsync(request.Slug, request.Sku, null, cancellationToken);
        await EnsureProductRelationsExistAsync(request, cancellationToken);

        var product = new Product();
        ApplyProduct(product, request);
        ApplyProductImages(product, request.Images);
        ApplyProductCategories(product, request.CategoryIds);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapProduct(product);
    }

    public async Task<AdminProductDto> UpdateProductAsync(
        Guid productId,
        UpsertProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await ProductQuery()
            .SingleOrDefaultAsync(entity => entity.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("محصول پیدا نشد.");

        await EnsureProductKeysAreUniqueAsync(request.Slug, request.Sku, productId, cancellationToken);
        await EnsureProductRelationsExistAsync(request, cancellationToken);

        ApplyProduct(product, request);
        SyncProductImages(product, request.Images);
        SyncProductCategories(product, request.CategoryIds);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapProduct(product);
    }

    public async Task<AdminProductDto> SetProductActiveAsync(
        Guid productId,
        SetProductActiveRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await ProductQuery()
            .SingleOrDefaultAsync(entity => entity.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("محصول پیدا نشد.");

        product.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapProduct(product);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(entity => entity.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("محصول پیدا نشد.");

        product.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminOrderSummaryDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Take(MaxAdminListItems)
            .ToListAsync(cancellationToken);

        return orders.Select(MapOrderSummary).ToArray();
    }

    public async Task<OrderDetailsDto?> GetOrderAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.Coupon)
            .Include(entity => entity.Items)
            .Include(entity => entity.Payments)
            .SingleOrDefaultAsync(entity => entity.OrderNumber == orderNumber, cancellationToken);

        return order is null ? null : CommerceMapping.ToOrderDetailsDto(order);
    }

    public async Task<AdminOrderSummaryDto> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException("سفارش پیدا نشد.");

        order.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapOrderSummary(order);
    }

    public async Task<IReadOnlyList<AdminCouponDto>> GetCouponsAsync(CancellationToken cancellationToken = default)
    {
        var coupons = await dbContext.Coupons
            .AsNoTracking()
            .OrderByDescending(coupon => coupon.CreatedAt)
            .Take(MaxAdminListItems)
            .ToListAsync(cancellationToken);

        return coupons.Select(MapCoupon).ToArray();
    }

    public async Task<AdminCouponDto> CreateCouponAsync(
        UpsertCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCouponCodeIsUniqueAsync(request.Code, null, cancellationToken);

        var coupon = new Coupon();
        ApplyCoupon(coupon, request);
        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapCoupon(coupon);
    }

    public async Task<AdminCouponDto> UpdateCouponAsync(
        Guid couponId,
        UpsertCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var coupon = await dbContext.Coupons
            .SingleOrDefaultAsync(entity => entity.Id == couponId, cancellationToken)
            ?? throw new InvalidOperationException("کوپن پیدا نشد.");

        await EnsureCouponCodeIsUniqueAsync(request.Code, couponId, cancellationToken);

        ApplyCoupon(coupon, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapCoupon(coupon);
    }

    public async Task DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await dbContext.Coupons
            .Include(entity => entity.Orders)
            .SingleOrDefaultAsync(entity => entity.Id == couponId, cancellationToken)
            ?? throw new InvalidOperationException("کوپن پیدا نشد.");

        if (coupon.Orders.Count > 0)
        {
            coupon.IsActive = false;
        }
        else
        {
            dbContext.Coupons.Remove(coupon);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminReviewDto>> GetReviewsAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Include(review => review.Product)
            .Include(review => review.User)
            .OrderByDescending(review => review.CreatedAt)
            .Take(MaxAdminListItems)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapReview).ToArray();
    }

    public async Task<AdminReviewDto> SetReviewApprovalAsync(
        Guid reviewId,
        SetReviewApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await dbContext.Reviews
            .Include(entity => entity.Product)
            .Include(entity => entity.User)
            .SingleOrDefaultAsync(entity => entity.Id == reviewId, cancellationToken)
            ?? throw new InvalidOperationException("نظر پیدا نشد.");

        review.IsApproved = request.IsApproved;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapReview(review);
    }

    public async Task<IReadOnlyList<AdminSliderDto>> GetSlidersAsync(CancellationToken cancellationToken = default)
    {
        var sliders = await dbContext.Sliders
            .AsNoTracking()
            .OrderBy(slider => slider.DisplayOrder)
            .ThenBy(slider => slider.Title)
            .ToListAsync(cancellationToken);

        return sliders.Select(MapSlider).ToArray();
    }

    public async Task<AdminSliderDto> CreateSliderAsync(
        UpsertSliderRequest request,
        CancellationToken cancellationToken = default)
    {
        var slider = new Slider();
        ApplySlider(slider, request);
        dbContext.Sliders.Add(slider);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapSlider(slider);
    }

    public async Task<AdminSliderDto> UpdateSliderAsync(
        Guid sliderId,
        UpsertSliderRequest request,
        CancellationToken cancellationToken = default)
    {
        var slider = await dbContext.Sliders
            .SingleOrDefaultAsync(entity => entity.Id == sliderId, cancellationToken)
            ?? throw new InvalidOperationException("اسلایدر پیدا نشد.");

        ApplySlider(slider, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapSlider(slider);
    }

    public async Task DeleteSliderAsync(Guid sliderId, CancellationToken cancellationToken = default)
    {
        var slider = await dbContext.Sliders
            .SingleOrDefaultAsync(entity => entity.Id == sliderId, cancellationToken)
            ?? throw new InvalidOperationException("اسلایدر پیدا نشد.");

        dbContext.Sliders.Remove(slider);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminPageDto>> GetPagesAsync(CancellationToken cancellationToken = default)
    {
        var pages = await dbContext.Pages
            .AsNoTracking()
            .OrderBy(page => page.Key)
            .ToListAsync(cancellationToken);

        return pages.Select(MapPage).ToArray();
    }

    public async Task<AdminPageDto> CreatePageAsync(
        UpsertPageRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePageKeysAreUniqueAsync(request.Key, request.Slug, null, cancellationToken);

        var page = new Page();
        ApplyPage(page, request);
        dbContext.Pages.Add(page);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapPage(page);
    }

    public async Task<AdminPageDto> UpdatePageAsync(
        Guid pageId,
        UpsertPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = await dbContext.Pages
            .SingleOrDefaultAsync(entity => entity.Id == pageId, cancellationToken)
            ?? throw new InvalidOperationException("صفحه پیدا نشد.");

        await EnsurePageKeysAreUniqueAsync(request.Key, request.Slug, pageId, cancellationToken);

        ApplyPage(page, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapPage(page);
    }

    public async Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var page = await dbContext.Pages
            .SingleOrDefaultAsync(entity => entity.Id == pageId, cancellationToken)
            ?? throw new InvalidOperationException("صفحه پیدا نشد.");

        dbContext.Pages.Remove(page);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminFaqItemDto>> GetFaqItemsAsync(CancellationToken cancellationToken = default)
    {
        var faqs = await dbContext.FaqItems
            .AsNoTracking()
            .OrderBy(faq => faq.DisplayOrder)
            .ThenBy(faq => faq.Question)
            .ToListAsync(cancellationToken);

        return faqs.Select(MapFaqItem).ToArray();
    }

    public async Task<AdminFaqItemDto> CreateFaqItemAsync(
        UpsertFaqItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var faq = new FaqItem();
        ApplyFaqItem(faq, request);
        dbContext.FaqItems.Add(faq);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapFaqItem(faq);
    }

    public async Task<AdminFaqItemDto> UpdateFaqItemAsync(
        Guid faqItemId,
        UpsertFaqItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var faq = await dbContext.FaqItems
            .SingleOrDefaultAsync(entity => entity.Id == faqItemId, cancellationToken)
            ?? throw new InvalidOperationException("سوال متداول پیدا نشد.");

        ApplyFaqItem(faq, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapFaqItem(faq);
    }

    public async Task DeleteFaqItemAsync(Guid faqItemId, CancellationToken cancellationToken = default)
    {
        var faq = await dbContext.FaqItems
            .SingleOrDefaultAsync(entity => entity.Id == faqItemId, cancellationToken)
            ?? throw new InvalidOperationException("سوال متداول پیدا نشد.");

        dbContext.FaqItems.Remove(faq);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminContactMessageDto>> GetContactMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        var messages = await dbContext.ContactMessages
            .AsNoTracking()
            .OrderBy(message => message.IsRead)
            .ThenByDescending(message => message.CreatedAt)
            .Take(MaxAdminListItems)
            .ToListAsync(cancellationToken);

        return messages.Select(MapContactMessage).ToArray();
    }

    public async Task<AdminContactMessageDto> MarkContactMessageAsReadAsync(
        Guid contactMessageId,
        CancellationToken cancellationToken = default)
    {
        var message = await dbContext.ContactMessages
            .SingleOrDefaultAsync(entity => entity.Id == contactMessageId, cancellationToken)
            ?? throw new InvalidOperationException("پیام تماس پیدا نشد.");

        message.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapContactMessage(message);
    }

    public async Task DeleteContactMessageAsync(Guid contactMessageId, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.ContactMessages
            .SingleOrDefaultAsync(entity => entity.Id == contactMessageId, cancellationToken)
            ?? throw new InvalidOperationException("پیام تماس پیدا نشد.");

        dbContext.ContactMessages.Remove(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminWebsiteSettingsDto> GetWebsiteSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateWebsiteSettingsAsync(cancellationToken);
        return MapWebsiteSettings(settings);
    }

    public async Task<AdminWebsiteSettingsDto> UpdateWebsiteSettingsAsync(
        UpdateWebsiteSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateWebsiteSettingsAsync(cancellationToken);

        settings.SiteName = request.SiteName.Trim();
        settings.LogoUrl = NormalizeOptional(request.LogoUrl);
        settings.SupportEmail = NormalizeOptional(request.SupportEmail);
        settings.SupportPhone = NormalizeOptional(request.SupportPhone);
        settings.Address = NormalizeOptional(request.Address);
        settings.SeoTitle = NormalizeOptional(request.SeoTitle);
        settings.SeoDescription = NormalizeOptional(request.SeoDescription);

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapWebsiteSettings(settings);
    }

    public async Task<IReadOnlyList<AdminQrLinkDto>> GetQrLinksAsync(CancellationToken cancellationToken = default)
    {
        var links = await dbContext.QRLinks
            .AsNoTracking()
            .Include(link => link.Products)
            .OrderBy(link => link.Label)
            .ThenBy(link => link.TargetUrl)
            .ToListAsync(cancellationToken);

        return links.Select(MapQrLink).ToArray();
    }

    public async Task<AdminQrLinkDto> CreateQrLinkAsync(
        UpsertQrLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = new QRLink();
        ApplyQrLink(link, request);
        dbContext.QRLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapQrLink(link);
    }

    public async Task<AdminQrLinkDto> UpdateQrLinkAsync(
        Guid qrLinkId,
        UpsertQrLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await dbContext.QRLinks
            .Include(entity => entity.Products)
            .SingleOrDefaultAsync(entity => entity.Id == qrLinkId, cancellationToken)
            ?? throw new InvalidOperationException("لینک QR پیدا نشد.");

        ApplyQrLink(link, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapQrLink(link);
    }

    public async Task DeleteQrLinkAsync(Guid qrLinkId, CancellationToken cancellationToken = default)
    {
        var link = await dbContext.QRLinks
            .Include(entity => entity.Products)
            .SingleOrDefaultAsync(entity => entity.Id == qrLinkId, cancellationToken)
            ?? throw new InvalidOperationException("لینک QR پیدا نشد.");

        if (link.Products.Count > 0)
        {
            link.IsActive = false;
        }
        else
        {
            dbContext.QRLinks.Remove(link);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> ProductQuery()
    {
        return dbContext.Products
            .AsSplitQuery()
            .Include(product => product.Images)
            .Include(product => product.ProductCategories)
            .ThenInclude(productCategory => productCategory.Category);
    }

    private async Task<WebsiteSettings> GetOrCreateWebsiteSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.WebsiteSettings
            .OrderBy(entity => entity.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new WebsiteSettings();
        dbContext.WebsiteSettings.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    private async Task EnsureCategorySlugIsUniqueAsync(
        string slug,
        Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim();
        var exists = await dbContext.Categories
            .AnyAsync(category => category.Slug == normalizedSlug && category.Id != categoryId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("اسلاگ دسته‌بندی قبلا استفاده شده است.");
        }
    }

    private async Task EnsureParentCategoryExistsAsync(
        Guid? parentCategoryId,
        Guid? categoryId,
        CancellationToken cancellationToken)
    {
        if (parentCategoryId is null)
        {
            return;
        }

        if (parentCategoryId == categoryId)
        {
            throw new InvalidOperationException("یک دسته‌بندی نمی‌تواند والد خودش باشد.");
        }

        var exists = await dbContext.Categories
            .AnyAsync(category => category.Id == parentCategoryId, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("دسته‌بندی والد پیدا نشد.");
        }
    }

    private async Task EnsureProductKeysAreUniqueAsync(
        string slug,
        string sku,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim();
        var normalizedSku = sku.Trim();
        var exists = await dbContext.Products.AnyAsync(
            product =>
                product.Id != productId &&
                (product.Slug == normalizedSlug || product.Sku == normalizedSku),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("اسلاگ یا SKU محصول قبلا استفاده شده است.");
        }
    }

    private async Task EnsureProductRelationsExistAsync(
        UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QrLinkId is not null)
        {
            var qrExists = await dbContext.QRLinks.AnyAsync(link => link.Id == request.QrLinkId, cancellationToken);
            if (!qrExists)
            {
                throw new InvalidOperationException("لینک QR پیدا نشد.");
            }
        }

        var categoryIds = request.CategoryIds.Distinct().ToArray();
        if (categoryIds.Length == 0)
        {
            return;
        }

        var foundCategoryCount = await dbContext.Categories
            .CountAsync(category => categoryIds.Contains(category.Id), cancellationToken);

        if (foundCategoryCount != categoryIds.Length)
        {
            throw new InvalidOperationException("یک یا چند دسته‌بندی پیدا نشد.");
        }
    }

    private async Task EnsurePageKeysAreUniqueAsync(
        string key,
        string slug,
        Guid? pageId,
        CancellationToken cancellationToken)
    {
        var normalizedKey = key.Trim();
        var normalizedSlug = slug.Trim();
        var exists = await dbContext.Pages.AnyAsync(
            page =>
                page.Id != pageId &&
                (page.Key == normalizedKey || page.Slug == normalizedSlug),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("کلید یا اسلاگ صفحه قبلا استفاده شده است.");
        }
    }

    private async Task EnsureCouponCodeIsUniqueAsync(
        string code,
        Guid? couponId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = await dbContext.Coupons
            .AnyAsync(coupon => coupon.Code == normalizedCode && coupon.Id != couponId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("کد کوپن قبلا استفاده شده است.");
        }
    }

    private static void ApplyCategory(Category category, UpsertCategoryRequest request)
    {
        category.Name = request.Name.Trim();
        category.Slug = request.Slug.Trim();
        category.Description = NormalizeOptional(request.Description);
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
    }

    private static void ApplyProduct(Product product, UpsertProductRequest request)
    {
        product.Name = request.Name.Trim();
        product.Slug = request.Slug.Trim();
        product.Sku = request.Sku.Trim();
        product.Description = NormalizeOptional(request.Description);
        product.Specifications = NormalizeOptional(request.Specifications);
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.IsBestSeller = request.IsBestSeller;
        product.QrLinkId = request.QrLinkId;
    }

    private static void ApplyProductImages(
        Product product,
        IReadOnlyList<UpsertProductImageRequest> images)
    {
        var normalizedImages = images
            .Where(image => !string.IsNullOrWhiteSpace(image.Url))
            .OrderBy(image => image.DisplayOrder)
            .Take(10)
            .ToArray();

        var primaryWasSet = false;
        foreach (var image in normalizedImages)
        {
            var isPrimary = image.IsPrimary && !primaryWasSet;
            primaryWasSet |= isPrimary;

            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                Url = image.Url.Trim(),
                AltText = NormalizeOptional(image.AltText),
                DisplayOrder = image.DisplayOrder,
                IsPrimary = isPrimary
            });
        }

        if (!primaryWasSet && product.Images.Count > 0)
        {
            product.Images.OrderBy(image => image.DisplayOrder).First().IsPrimary = true;
        }
    }

    private void SyncProductImages(Product product, IReadOnlyList<UpsertProductImageRequest> images)
    {
        var normalizedImages = images
            .Where(image => !string.IsNullOrWhiteSpace(image.Url))
            .OrderBy(image => image.DisplayOrder)
            .Take(10)
            .ToArray();
        var existingImages = product.Images
            .OrderBy(image => image.DisplayOrder)
            .ToArray();

        var primaryWasSet = false;
        for (var index = 0; index < normalizedImages.Length; index++)
        {
            var request = normalizedImages[index];
            var isPrimary = request.IsPrimary && !primaryWasSet;
            primaryWasSet |= isPrimary;

            if (index < existingImages.Length)
            {
                var existing = existingImages[index];
                existing.Url = request.Url.Trim();
                existing.AltText = NormalizeOptional(request.AltText);
                existing.DisplayOrder = request.DisplayOrder;
                existing.IsPrimary = isPrimary;
            }
            else
            {
                var newImage = new ProductImage
                {
                    ProductId = product.Id,
                    Url = request.Url.Trim(),
                    AltText = NormalizeOptional(request.AltText),
                    DisplayOrder = request.DisplayOrder,
                    IsPrimary = isPrimary
                };

                product.Images.Add(newImage);
                dbContext.ProductImages.Add(newImage);
            }
        }

        foreach (var removedImage in existingImages.Skip(normalizedImages.Length))
        {
            dbContext.ProductImages.Remove(removedImage);
            product.Images.Remove(removedImage);
        }

        if (!primaryWasSet && product.Images.Count > 0)
        {
            product.Images.OrderBy(image => image.DisplayOrder).First().IsPrimary = true;
        }
    }

    private static void ApplyProductCategories(Product product, IReadOnlyList<Guid> categoryIds)
    {
        foreach (var categoryId in categoryIds.Distinct())
        {
            product.ProductCategories.Add(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = categoryId
            });
        }
    }

    private void SyncProductCategories(Product product, IReadOnlyList<Guid> categoryIds)
    {
        var requestedCategoryIds = categoryIds.Distinct().ToHashSet();
        var removedCategories = product.ProductCategories
            .Where(productCategory => !requestedCategoryIds.Contains(productCategory.CategoryId))
            .ToArray();

        foreach (var productCategory in removedCategories)
        {
            product.ProductCategories.Remove(productCategory);
        }

        var existingCategoryIds = product.ProductCategories
            .Select(productCategory => productCategory.CategoryId)
            .ToHashSet();

        foreach (var categoryId in requestedCategoryIds.Except(existingCategoryIds))
        {
            var productCategory = new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = categoryId
            };

            product.ProductCategories.Add(productCategory);
            dbContext.ProductCategories.Add(productCategory);
        }
    }

    private static void ApplySlider(Slider slider, UpsertSliderRequest request)
    {
        slider.Title = request.Title.Trim();
        slider.Subtitle = NormalizeOptional(request.Subtitle);
        slider.ImageUrl = request.ImageUrl.Trim();
        slider.LinkUrl = NormalizeOptional(request.LinkUrl);
        slider.DisplayOrder = request.DisplayOrder;
        slider.IsActive = request.IsActive;
    }

    private static void ApplyCoupon(Coupon coupon, UpsertCouponRequest request)
    {
        coupon.Code = request.Code.Trim().ToUpperInvariant();
        coupon.DiscountAmount = request.DiscountAmount;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.StartsAt = request.StartsAt;
        coupon.EndsAt = request.EndsAt;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;
    }

    private static void ApplyPage(Page page, UpsertPageRequest request)
    {
        page.Key = request.Key.Trim();
        page.Title = request.Title.Trim();
        page.Slug = request.Slug.Trim();
        page.Content = request.Content;
        page.MetaTitle = NormalizeOptional(request.MetaTitle);
        page.MetaDescription = NormalizeOptional(request.MetaDescription);
        page.IsPublished = request.IsPublished;
    }

    private static void ApplyFaqItem(FaqItem faq, UpsertFaqItemRequest request)
    {
        faq.Question = request.Question.Trim();
        faq.Answer = request.Answer.Trim();
        faq.DisplayOrder = request.DisplayOrder;
        faq.IsActive = request.IsActive;
    }

    private static void ApplyQrLink(QRLink link, UpsertQrLinkRequest request)
    {
        link.TargetUrl = request.TargetUrl.Trim();
        link.Label = NormalizeOptional(request.Label);
        link.IsActive = request.IsActive;
    }

    private static AdminCategoryDto MapCategory(Category category)
    {
        return new AdminCategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.IsActive,
            category.SortOrder,
            category.ProductCategories.Count,
            category.CreatedAt);
    }

    private static AdminProductDto MapProduct(Product product)
    {
        return new AdminProductDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.Description,
            product.Specifications,
            product.Price,
            product.DiscountPrice,
            product.Stock,
            product.IsActive,
            product.IsFeatured,
            product.IsBestSeller,
            product.QrLinkId,
            product.ProductCategories
                .OrderBy(productCategory => productCategory.Category?.SortOrder ?? int.MaxValue)
                .ThenBy(productCategory => productCategory.CategoryId)
                .Select(productCategory => productCategory.CategoryId)
                .ToArray(),
            product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Select(MapProductImage)
                .ToArray(),
            product.CreatedAt);
    }

    private static AdminProductImageDto MapProductImage(ProductImage image)
    {
        return new AdminProductImageDto(
            image.Id,
            image.Url,
            image.AltText,
            image.DisplayOrder,
            image.IsPrimary);
    }

    private static AdminOrderSummaryDto MapOrderSummary(Order order)
    {
        return new AdminOrderSummaryDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.CustomerFullName,
            order.CustomerEmail,
            order.TotalAmount,
            order.Items.Sum(item => item.Quantity),
            order.CreatedAt);
    }

    private static AdminReviewDto MapReview(Review review)
    {
        return new AdminReviewDto(
            review.Id,
            review.Product.Name,
            review.User.FullName,
            review.Rating,
            review.Comment,
            review.IsApproved,
            review.CreatedAt);
    }

    private static AdminCouponDto MapCoupon(Coupon coupon)
    {
        return new AdminCouponDto(
            coupon.Id,
            coupon.Code,
            coupon.DiscountAmount,
            coupon.MinimumOrderAmount,
            coupon.StartsAt,
            coupon.EndsAt,
            coupon.UsageLimit,
            coupon.UsedCount,
            coupon.IsActive,
            coupon.CreatedAt);
    }

    private static AdminSliderDto MapSlider(Slider slider)
    {
        return new AdminSliderDto(
            slider.Id,
            slider.Title,
            slider.Subtitle,
            slider.ImageUrl,
            slider.LinkUrl,
            slider.DisplayOrder,
            slider.IsActive);
    }

    private static AdminPageDto MapPage(Page page)
    {
        return new AdminPageDto(
            page.Id,
            page.Key,
            page.Title,
            page.Slug,
            page.Content,
            page.MetaTitle,
            page.MetaDescription,
            page.IsPublished);
    }

    private static AdminFaqItemDto MapFaqItem(FaqItem faq)
    {
        return new AdminFaqItemDto(
            faq.Id,
            faq.Question,
            faq.Answer,
            faq.DisplayOrder,
            faq.IsActive);
    }

    private static AdminContactMessageDto MapContactMessage(ContactMessage message)
    {
        return new AdminContactMessageDto(
            message.Id,
            message.FullName,
            message.Email,
            message.PhoneNumber,
            message.Subject,
            message.Message,
            message.IsRead,
            message.CreatedAt);
    }

    private static AdminWebsiteSettingsDto MapWebsiteSettings(WebsiteSettings settings)
    {
        return new AdminWebsiteSettingsDto(
            settings.Id,
            settings.SiteName,
            settings.LogoUrl,
            settings.SupportEmail,
            settings.SupportPhone,
            settings.Address,
            settings.SeoTitle,
            settings.SeoDescription);
    }

    private static AdminQrLinkDto MapQrLink(QRLink link)
    {
        return new AdminQrLinkDto(
            link.Id,
            link.TargetUrl,
            link.Label,
            link.IsActive,
            link.Products.Count);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
