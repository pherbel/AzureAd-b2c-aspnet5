using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Website.Controllers
{
    public class AccountController : Controller
    {

        private readonly AzureAdSettings _adSettigs;


        public AccountController(IOptions<AzureAdSettings> adSettigsOption)
        {
            if (adSettigsOption == null)
                throw new ArgumentNullException(nameof(adSettigsOption));
            if (adSettigsOption.Value == null)
                throw new ArgumentNullException(nameof(adSettigsOption.Value));

            _adSettigs = adSettigsOption.Value;
        }
        public IActionResult SignIn()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                return Challenge(
                    new AuthenticationProperties(new Dictionary<string, string> { { AuthenticationConstants.B2CPolicy, _adSettigs.B2CPolicySettings.SignInOrSignUpPolicy } }) { RedirectUri = "/" },
                    AuthenticationConstants.OpenIdConnectAzureAdB2CAuthenticationScheme);
            }

            return RedirectHome();
        }
        public IActionResult Profile()
         { 
             if (User.Identity.IsAuthenticated) 
             {
                return Challenge(
                    new AuthenticationProperties(new Dictionary<string, string> { { AuthenticationConstants.B2CPolicy, _adSettigs.B2CPolicySettings.EditProfilePolicy } }) { RedirectUri = "/" },
                    AuthenticationConstants.OpenIdConnectAzureAdB2CAuthenticationScheme);
             } 
 

             return RedirectHome(); 
         } 

        public IActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var callbackUrl = Url.Action("SignedOut", "Account", values: null, protocol: Request.Scheme);
                return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                    CookieAuthenticationDefaults.AuthenticationScheme, AuthenticationConstants.OpenIdConnectAzureAdB2CAuthenticationScheme);
            }
            return RedirectHome();
        }

        public IActionResult SignedOut()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectHome();
            }

            return View();
        }

        private IActionResult RedirectHome()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

    }
}
