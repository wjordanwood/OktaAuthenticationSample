using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OktaWebApp.Extensions.Authentication;

namespace OktaWebApp.Controllers;

public class PolicyController : BaseController
{
    // set by fallback policy
    public IActionResult RegularJoe()
    {
        return View();
    }
    
    [Authorize(AuthorizationPolicyNames.Manager)]
    public IActionResult Manager()
    {
        return View();
    }
    
    [Authorize(AuthorizationPolicyNames.Group1)]
    public IActionResult Group1()
    {
        return View();
    }
    
    [Authorize(AuthorizationPolicyNames.Group1Manager)]
    public IActionResult Group1Manager()
    {
        return View();
    }

    [Authorize(AuthorizationPolicyNames.Admin)]
    public IActionResult Admin()
    {
        return View();
    }
}