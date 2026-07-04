namespace SocialHub.Identity.Permissions;
 
/// <summary>
/// Starter permission catalog. Naming: Permissions.{Area}.{Action}. Every
/// later phase that introduces its own feature area (Posts, Communities,
/// Moderation, etc.) should append a nested static class here rather than
/// creating a parallel catalog, keeping role/claim seeding centralized.
/// </summary>
public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Manage = "Permissions.Users.Manage";
    }
 
    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Manage = "Permissions.Roles.Manage";
    }
 
    public static class System
    {
        public const string ViewAuditLog = "Permissions.System.ViewAuditLog";
        public const string ManageConfiguration = "Permissions.System.ManageConfiguration";
    }
 
    public static IReadOnlyList<string> All { get; } = new[]
    {
        Users.View, Users.Manage,
        Roles.View, Roles.Manage,
        System.ViewAuditLog, System.ManageConfiguration
    };
}