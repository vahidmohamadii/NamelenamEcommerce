using NaderEcommerce.Domain.Catalog;
using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Orders;

namespace NaderEcommerce.Domain.Identity;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockoutEndAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    public ShoppingCart? ShoppingCart { get; set; }
    public ICollection<Order> Orders { get; } = new List<Order>();
    public ICollection<Review> Reviews { get; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; } = new List<WishlistItem>();
}
