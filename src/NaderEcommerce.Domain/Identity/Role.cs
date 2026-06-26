using NaderEcommerce.Domain.Common;

namespace NaderEcommerce.Domain.Identity;

public sealed class Role : BaseEntity
{
    public const string Customer = "Customer";
    public const string Administrator = "Admin";

    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
}
