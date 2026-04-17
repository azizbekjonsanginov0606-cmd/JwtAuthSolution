namespace Infrastructure.Identity;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] AllRoles = [Admin, Manager, User];
}
