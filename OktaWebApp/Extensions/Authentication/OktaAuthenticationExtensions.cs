using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Okta.AspNetCore;

namespace OktaWebApp.Extensions.Authentication;

public static class OktaAuthenticationExtensions {
    public static WebApplicationBuilder AddOktaAuthentication(this WebApplicationBuilder builder) {
	    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOktaMvc(new OktaMvcOptions
        {
            OktaDomain = builder.Configuration.GetValue<string>("Okta:OktaDomain"),
            ClientId = builder.Configuration.GetValue<string>("Okta:ClientId"),
            ClientSecret = builder.Configuration.GetValue<string>("Okta:ClientSecret"),
            AuthorizationServerId = builder.Configuration.GetValue<string>("Okta:AuthorizationServerId"),
            Scope = new List<string> { "openid", "profile", "email", "groups" },
            OpenIdConnectEvents = new OpenIdConnectEvents()
            {
                OnUserInformationReceived = context => TurnOktaGroupsIntoIdentityRoles(context)
            }
        });

        builder.Services.AddAuthorization(options =>
        {
            foreach (var policy in NamedAuthorizationPolicies.EnabledPolicies)
            {
                options.AddPolicy(policy.Name, policy.Policy);
            }
            options.FallbackPolicy = NamedAuthorizationPolicies.GeneralUserPolicy.Policy;
        });

        return builder;
    }
    /// <summary>
    /// Task that will be run during okta authorization callback, forming the okta "group" membership into usable roles
    /// that we can assign and have access to during application flow using standard .net authorization policies
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static Task TurnOktaGroupsIntoIdentityRoles(UserInformationReceivedContext context)
    {
        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.ReadJwtToken(context.ProtocolMessage.AccessToken);

        var groups = accessToken.Claims.Where(c => c.Type == "groups").ToList();
        if (groups.Any())
        {
            ClaimsIdentity claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            foreach (var group in groups)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, group.Value));
            }
        }
        
        return Task.CompletedTask;
    }
}