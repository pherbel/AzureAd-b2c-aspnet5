using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Website.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        private const string EmailAddressClaim = "emails";
        private const string NameClaim = "name";
        private const string UserIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public static string GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(UserIdClaim);
            if (claim != null)
            {
                return user.FindFirst(UserIdClaim).Value;

            }
            return string.Empty;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(NameClaim);
            if (claim != null)
            {
                return user.FindFirst(NameClaim).Value;

            }
            return string.Empty;
        }

        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(EmailAddressClaim);
            if (claim != null)
            {
                return user.FindFirst(EmailAddressClaim).Value;

            }
            return string.Empty;
        }


    }
}
