using NaderEcommerce.Domain.Common;
using NaderEcommerce.Domain.Identity;

namespace NaderEcommerce.Domain.Catalog;

public sealed class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
}
