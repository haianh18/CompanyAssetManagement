using Microsoft.AspNetCore.Identity;

public class AppRole : IdentityRole<int>
{
    public string? Description { get; set; }
    public RoleType RoleType { get; set; }

    public string GetRoleDescription()
    {
        return RoleType.GetDescription();
    }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}

