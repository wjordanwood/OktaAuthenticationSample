using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace OktaWebApp.Extensions.Authentication;

// Okta groups are turned into ClaimsIdentity Roles upon returning from Okta SSO
public static class OktaGroups
{
    public const string Group1 = "webapp.Group1";
    public const string Admin = "webapp.Admins";
    public const string Manager = "webapp.Managers";
}

// Names for authorization policies to keep convention consistent throughout all uses
public static class AuthorizationPolicyNames
{
    public const string GeneralUser = "GeneralUserPolicy";
    public const string Group1 = "Group1UserPolicy";
    public const string Group1Manager = "Group1ManagerPolicy";
    public const string Manager = "ManagerPolicy";
    public const string Admin = "AdminPolicy";
}

public static class NamedAuthorizationPolicies
{
    // simplify bloat of "Add Policy" by making it iterative
    public static List<NamedAuthorizationPolicy> EnabledPolicies =>
    [
        GeneralUserPolicy, GroupOnePolicy, GroupOneManagerPolicy, ManagerPolicy, AdminPolicy
    ];
    
    // NCC: Define Authorization Policies that will be added during startup to be referenced in controllers using [Authorize(PolicyName)]
    #region Policy Definitions
    // Used as the Fallback Policy, meaning that unless you specify
    //      a higher group policy to further restrict access
    //      or AllowAnonymous to allow anyone on a method,
    // you have to have an authenticated user at least.
    public static NamedAuthorizationPolicy GeneralUserPolicy =>
        new(AuthorizationPolicyNames.GeneralUser,
            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

    public static NamedAuthorizationPolicy GroupOnePolicy =>
        new(AuthorizationPolicyNames.Group1,
            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(OktaGroups.Group1)
                .Build());
    
    public static NamedAuthorizationPolicy GroupOneManagerPolicy => 
        new(AuthorizationPolicyNames.Group1Manager,
            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(OktaGroups.Group1)
                .RequireRole(OktaGroups.Manager)
                .Build());
    
    public static NamedAuthorizationPolicy ManagerPolicy =>
        new(AuthorizationPolicyNames.Manager,
            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(OktaGroups.Manager)
                .Build());
    public static NamedAuthorizationPolicy AdminPolicy =>
        new(AuthorizationPolicyNames.Admin,
            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(OktaGroups.Admin)
                .Build());
    #endregion
}

/// <summary>
/// simplify authorization policy declaration by making an object that we can make a bunch of and iterate through
/// </summary>
/// <param name="name"></param>
/// <param name="policy"></param>
public class NamedAuthorizationPolicy(string name, AuthorizationPolicy policy)
{
    public string Name { get; } = name;
    public AuthorizationPolicy Policy { get; } = policy;
}

/// <summary>
/// Used on View to dynamically render UI, you tell it the roles that are required for a link to show and then use HasRequiredRoles to check before rendering HTML
/// </summary>
/// <param name="linkText"></param>
/// <param name="redirectAction"></param>
/// <param name="requiredRoles"></param>
public class RequiredRoleLink(string linkText, string redirectAction, List<string>? requiredRoles)
{
    public List<string>? RequiredRoles { get; set; } = requiredRoles;
    public string LinkText { get; set; } = linkText;
    public string RedirectAction { get; set; } = redirectAction;

    public bool HasRequiredRoles(ClaimsIdentity identity)
    {
        if (RequiredRoles.IsNullOrEmpty()) return true;

        var userClaims = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        if (userClaims.IsNullOrEmpty()) return false;

        return RequiredRoles.All(r => userClaims.Contains(r));
    }
}