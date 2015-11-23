using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.IdentityModel.Tokens;

namespace Website.PolicyAuthHelpers
{
    public class PolicyConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
    {
        private const string policyParameter = "p";

        private readonly Dictionary<string, IConfigurationManager<OpenIdConnectConfiguration>> _policyCOnfigurationManagers = new Dictionary<string, IConfigurationManager<OpenIdConnectConfiguration>>();

        public PolicyConfigurationManager(string metadataAddress, string[] policies)
        {
            foreach (var policy in policies)
            {
                _policyCOnfigurationManagers.Add(policy, new ConfigurationManager<OpenIdConnectConfiguration>(String.Format(metadataAddress + "?{0}={1}", policyParameter, policy), new OpenIdConnectConfigurationRetriever()));
            }
        }

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            OpenIdConnectConfiguration configUnion = new OpenIdConnectConfiguration();
            foreach (KeyValuePair<string, IConfigurationManager<OpenIdConnectConfiguration>> configManager in _policyCOnfigurationManagers)
            {
                OpenIdConnectConfiguration config = await configManager.Value.GetConfigurationAsync(cancel);
                configUnion = MergeConfig(configUnion, config);

            }
            return configUnion;
        }

        // Takes the ohter and copies it to source, preserving the source's multi-valued attributes as a running sum.
        private OpenIdConnectConfiguration MergeConfig(OpenIdConnectConfiguration source, OpenIdConnectConfiguration other)
        {

            //  ICollection<SecurityToken> existingSigningTokens = source.SigningTokens;
            ICollection<string> existingAlgs = source.IdTokenSigningAlgValuesSupported;
            ICollection<SecurityKey> existingSigningKeys = source.SigningKeys;

            //foreach (SecurityToken token in existingSigningTokens)
            //{
            //    other.SigningTokens.Add(token);
            //}

            foreach (string alg in existingAlgs)
            {
                other.IdTokenSigningAlgValuesSupported.Add(alg);
            }

            foreach (SecurityKey key in existingSigningKeys)
            {
                other.SigningKeys.Add(key);
            }

            return other;
        }

        public Task<OpenIdConnectConfiguration> GetConfigurationByPolicyAsync(CancellationToken cancel, string policyId)
        {
            if (string.IsNullOrEmpty(policyId))
                throw new ArgumentNullException(nameof(policyId));

            IConfigurationManager<OpenIdConnectConfiguration> configManager;
            if (_policyCOnfigurationManagers.TryGetValue(policyId, out configManager))
            {
                return configManager.GetConfigurationAsync(cancel);
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid policy id! Id: {0}", policyId));
        }

        public void RequestRefresh(string policyId)
        {
            if (string.IsNullOrEmpty(policyId))
                throw new ArgumentNullException(nameof(policyId));

            IConfigurationManager<OpenIdConnectConfiguration> configManager;
            if (_policyCOnfigurationManagers.TryGetValue(policyId, out configManager))
            {
                configManager.RequestRefresh();
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid policy id! Id: {0}", policyId));

        }

        public void RequestRefresh()
        {
            foreach (KeyValuePair<string, IConfigurationManager<OpenIdConnectConfiguration>> configManager in _policyCOnfigurationManagers)
            {
                configManager.Value.RequestRefresh();
            }
        }
    }
}
