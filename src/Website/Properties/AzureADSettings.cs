using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Properties
{
    public class AzureADSettings
    {
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string AadInstance { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string RedirectUri { get; set; }
        public string SignUpPolicyId { get; set; }
        public string SignInPolicyId { get; set; }
        public string UserProfilePolicyId { get; set; }
    }
}
