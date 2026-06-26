using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Infrastructure.Commerce;

internal static class CommercePricing
{
    private const decimal ShippingBaseAmount = 120000m;
    private const decimal FreeShippingThreshold = 2000000m;
    private const decimal TaxRate = 0.09m;

    public static PricingBreakdown Calculate(ShoppingCart cart)
    {
        var subtotal = cart.Items.Sum(item => GetUnitPrice(item.Product) * item.Quantity);
        var discount = CalculateDiscount(cart.Coupon, subtotal);
        var shipping = subtotal >= FreeShippingThreshold ? 0m : ShippingBaseAmount;
        var taxableAmount = Math.Max(0m, subtotal - discount);
        var tax = Math.Round(taxableAmount * TaxRate, 2, MidpointRounding.AwayFromZero);

        return new PricingBreakdown(subtotal, discount, shipping, tax, taxableAmount + shipping + tax);
    }

    public static decimal GetUnitPrice(Domain.Catalog.Product product)
    {
        return product.DiscountPrice ?? product.Price;
    }

    public static bool IsCouponUsable(Coupon? coupon, decimal subtotal)
    {
        if (coupon is null || !coupon.IsActive)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        if (coupon.StartsAt is not null && coupon.StartsAt > now)
        {
            return false;
        }

        if (coupon.EndsAt is not null && coupon.EndsAt < now)
        {
            return false;
        }

        if (coupon.UsageLimit is not null && coupon.UsedCount >= coupon.UsageLimit.Value)
        {
            return false;
        }

        if (coupon.MinimumOrderAmount is not null && subtotal < coupon.MinimumOrderAmount.Value)
        {
            return false;
        }

        return true;
    }

    private static decimal CalculateDiscount(Coupon? coupon, decimal subtotal)
    {
        if (!IsCouponUsable(coupon, subtotal))
        {
            return 0m;
        }

        return Math.Min(subtotal, coupon!.DiscountAmount);
    }
}
