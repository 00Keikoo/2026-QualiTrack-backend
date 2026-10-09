using QualiTrack.Models;

namespace QualiTrack.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string QualityManager = "QualityManager";
    public const string AuditorInternal = "AuditorInternal";
    public const string Auditee = "Auditee";
    
    public static readonly string[] AllRoles = { Admin, QualityManager, AuditorInternal, Auditee };
    
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(NormalizeRole(role));
    }

    public static string NormalizeRole(string role)
    {
        return role == "Auditor" ? AuditorInternal : role;
    }

    public static string GetClaimRole(string role)
    {
        return NormalizeRole(role);
    }
}
public static class UserStatuses
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly string[] AllStatuses = { Active, Inactive };

    public static bool IsValidStatus(string status)
    {
        return AllStatuses.Contains(status);
    }
}
public static class AdminActions
{
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
    public const string Activate = "ACTIVATE";
    public const string Deactivate = "DEACTIVATE";
}