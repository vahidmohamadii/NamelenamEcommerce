using FluentValidation;

namespace NaderEcommerce.Application.Admin;

public sealed class UpsertCategoryRequestValidator : AbstractValidator<UpsertCategoryRequest>
{
    public UpsertCategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(220);
        RuleFor(request => request.Description).MaximumLength(1000);
    }
}

public sealed class UpsertProductRequestValidator : AbstractValidator<UpsertProductRequest>
{
    public UpsertProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(220);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(260);
        RuleFor(request => request.Sku).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DiscountPrice)
            .GreaterThanOrEqualTo(0)
            .When(request => request.DiscountPrice is not null);
        RuleFor(request => request.Stock).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Images).Must(images => images.Count <= 10)
            .WithMessage("برای هر محصول حداکثر ۱۰ تصویر مجاز است.");
        RuleForEach(request => request.Images).SetValidator(new UpsertProductImageRequestValidator());
    }
}

public sealed class UpsertProductImageRequestValidator : AbstractValidator<UpsertProductImageRequest>
{
    public UpsertProductImageRequestValidator()
    {
        RuleFor(request => request.Url).NotEmpty().MaximumLength(2048);
        RuleFor(request => request.AltText).MaximumLength(220);
    }
}

public sealed class UpsertSliderRequestValidator : AbstractValidator<UpsertSliderRequest>
{
    public UpsertSliderRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(220);
        RuleFor(request => request.Subtitle).MaximumLength(500);
        RuleFor(request => request.ImageUrl).NotEmpty().MaximumLength(2048);
        RuleFor(request => request.LinkUrl).MaximumLength(2048);
    }
}

public sealed class UpsertCouponRequestValidator : AbstractValidator<UpsertCouponRequest>
{
    public UpsertCouponRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64);
        RuleFor(request => request.DiscountAmount).GreaterThan(0);
        RuleFor(request => request.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(request => request.MinimumOrderAmount is not null);
        RuleFor(request => request.UsageLimit)
            .GreaterThan(0)
            .When(request => request.UsageLimit is not null);
        RuleFor(request => request.EndsAt)
            .GreaterThan(request => request.StartsAt)
            .When(request => request.StartsAt is not null && request.EndsAt is not null);
    }
}

public sealed class UpsertPageRequestValidator : AbstractValidator<UpsertPageRequest>
{
    public UpsertPageRequestValidator()
    {
        RuleFor(request => request.Key).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(220);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(220);
        RuleFor(request => request.MetaTitle).MaximumLength(220);
        RuleFor(request => request.MetaDescription).MaximumLength(500);
    }
}

public sealed class UpsertFaqItemRequestValidator : AbstractValidator<UpsertFaqItemRequest>
{
    public UpsertFaqItemRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty().MaximumLength(300);
        RuleFor(request => request.Answer).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateWebsiteSettingsRequestValidator : AbstractValidator<UpdateWebsiteSettingsRequest>
{
    public UpdateWebsiteSettingsRequestValidator()
    {
        RuleFor(request => request.SiteName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.LogoUrl).MaximumLength(2048);
        RuleFor(request => request.SupportEmail).MaximumLength(256).EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.SupportEmail));
        RuleFor(request => request.SupportPhone).MaximumLength(32);
        RuleFor(request => request.SeoTitle).MaximumLength(220);
        RuleFor(request => request.SeoDescription).MaximumLength(500);
    }
}

public sealed class UpsertQrLinkRequestValidator : AbstractValidator<UpsertQrLinkRequest>
{
    public UpsertQrLinkRequestValidator()
    {
        RuleFor(request => request.TargetUrl).NotEmpty().MaximumLength(2048);
        RuleFor(request => request.Label).MaximumLength(160);
    }
}
