using Microsoft.AspNetCore.Identity;
 
namespace SocialHub.Identity.Models;
 
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
 
    public ApplicationRole()
    {
    }
 
    public ApplicationRole(string roleName, string? description = null)
        : base(roleName)
    {
        Description = description;
    }
}