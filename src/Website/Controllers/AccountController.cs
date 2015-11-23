using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Extensions.OptionsModel;
using Website.Properties;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication.Cookies;
using System.Security.Claims;

namespace Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly AzureADSettings _azureADSettings;

        public AccountController(IOptions<AzureADSettings> azureADSettings)
        {
            _azureADSettings = azureADSettings.Value;
        }

        public IActionResult SignIn()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string> {
                             {Startup.PolicyKey, _azureADSettings.SignInPolicyId}
                         })
                    { RedirectUri = "/Account/Index" });
            return RedirectToAction("Index", "Account");

        }

        public IActionResult SignUp()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string> {
                             {Startup.PolicyKey, _azureADSettings.SignUpPolicyId}
                         })
                    { RedirectUri = "/Account/Index" });
            return RedirectToAction("Index", "Account");
        }

        public IActionResult Profile()
        {
            if (HttpContext.User == null || HttpContext.User.Identity.IsAuthenticated)
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string> {
                             {Startup.PolicyKey, _azureADSettings.UserProfilePolicyId}
                         })
                    { RedirectUri = "/Account/Index" });
            return RedirectToAction("Index", "Account");
        }

        public IActionResult SignOut()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string> {
                         {Startup.PolicyKey, ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value}
                     })
                    { RedirectUri = "/Account/Index" });

                HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string> {
                         {Startup.PolicyKey, ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value}
                     })
                    { RedirectUri = "/Account/Index" });
            }
            return RedirectToAction("Index", "Account");

        }

        public IActionResult Index()
        {
            return View();

        }
    }
}
