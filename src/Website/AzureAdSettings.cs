using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website
{
    public class AzureAdSettings
    {

        public string AADInstance { get; set; }
        public string TenantId { get; set; }

        public string Domain { get; set; }

        public string Authority => $"{AADInstance}{Domain}/v2.0";

        public string ClientId { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public B2CPolicySettings B2CPolicySettings { get; set; }
    }
}
