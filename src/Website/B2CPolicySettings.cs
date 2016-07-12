using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website
{
    public class B2CPolicySettings
    {
        public string SignInOrSignUpPolicy { get; set; } 
        public string EditProfilePolicy { get; set; }

        public IEnumerable<string> GetPolicies()
        {
            return new[] { SignInOrSignUpPolicy, EditProfilePolicy };
        }
    }
}
