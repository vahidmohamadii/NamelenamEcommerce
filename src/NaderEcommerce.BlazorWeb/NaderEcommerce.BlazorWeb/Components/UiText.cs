using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.BlazorWeb.Components;

public static class UiText
{
    public static string OrderStatus(OrderStatus status)
        => status switch
        {
            NaderEcommerce.Domain.Orders.OrderStatus.Pending => "در انتظار",
            NaderEcommerce.Domain.Orders.OrderStatus.Paid => "پرداخت‌شده",
            NaderEcommerce.Domain.Orders.OrderStatus.Preparing => "در حال آماده‌سازی",
            NaderEcommerce.Domain.Orders.OrderStatus.Shipping => "در حال ارسال",
            NaderEcommerce.Domain.Orders.OrderStatus.Completed => "تکمیل‌شده",
            NaderEcommerce.Domain.Orders.OrderStatus.Cancelled => "لغوشده",
            NaderEcommerce.Domain.Orders.OrderStatus.Refunded => "مرجوع‌شده",
            _ => status.ToString()
        };

    public static string PaymentStatus(PaymentStatus status)
        => status switch
        {
            NaderEcommerce.Domain.Orders.PaymentStatus.Pending => "در انتظار",
            NaderEcommerce.Domain.Orders.PaymentStatus.Succeeded => "موفق",
            NaderEcommerce.Domain.Orders.PaymentStatus.Failed => "ناموفق",
            NaderEcommerce.Domain.Orders.PaymentStatus.Refunded => "مرجوع‌شده",
            _ => status.ToString()
        };

    public static string Role(string role)
        => role switch
        {
            "Admin" => "ادمین",
            "Customer" => "مشتری",
            _ => role
        };

    public static string Roles(IEnumerable<string> roles)
        => string.Join("، ", roles.Select(Role));
}
