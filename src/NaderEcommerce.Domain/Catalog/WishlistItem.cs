using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Identity;

namespace NaderEcommerce.Domain.Catalog;

public sealed class WishlistItem : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
