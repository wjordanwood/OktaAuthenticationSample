using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okta.AspNetCore;
using OktaWebApp.Models;
using Claim = OktaWebApp.Models.Claim;

namespace OktaWebApp.Controllers;

public class AccountController : BaseController
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    #region Authentication Flow
    [AllowAnonymous]
    public IActionResult SignIn()
    {
        if (!HttpContext.User.Identity.IsAuthenticated)
        {
            var properties = new AuthenticationProperties();

            return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult SignOut()
    {
        return new SignOutResult(
            new[]
            {
                OktaDefaults.MvcAuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme,
            },
            new AuthenticationProperties { RedirectUri = "/Home/" });
    }
    #endregion

    public IActionResult Current()
    {
        var flattenedClaims = (User.Identity as ClaimsIdentity).Claims.Select(c => 
            new Claim()
            {
                Type = c.Type, 
                Value = c.Value
            }).ToList();
        
        return View(new CurrentUserViewModel{ Claims = flattenedClaims });
    }
}