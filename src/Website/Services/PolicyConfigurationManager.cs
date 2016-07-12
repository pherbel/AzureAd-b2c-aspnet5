using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Website.Services
{
    public class PolicyConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
    {
        private const string policyParameter = "p";

        private readonly AzureAdSettings _adSettigs;

        private readonly Dictionary<string, IConfigurationManager<OpenIdConnectConfiguration>> _policyConfigurationManagers = new Dictionary<string, IConfigurationManager<OpenIdConnectConfiguration>>();

        public PolicyConfigurationManager(IOptions<AzureAdSettings> adSettigsOption)
        {
            if (adSettigsOption == null)
                throw new ArgumentNullException(nameof(adSettigsOption));
            if (adSettigsOption.Value == null)
                throw new ArgumentNullException(nameof(adSettigsOption.Value));

            _adSettigs = adSettigsOption.Value;
            foreach (var policy in _adSettigs.B2CPolicySettings.GetPolicies())
            {
                var metadataAddress = $"{_adSettigs.Authority}/{OpenIdProviderMetadataNames.Discovery}?{policyParameter}={policy}";
                _policyConfigurationManagers.Add(policy.ToLowerInvariant(), new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever()));
            }
        }

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            OpenIdConnectConfiguration configUnion = null;
            foreach (KeyValuePair<string, IConfigurationManager<OpenIdConnectConfiguration>> configManager in _policyConfigurationManagers)
            {
                var configuration = await configManager.Value.GetConfigurationAsync(cancel);
                if (configUnion == null)
                    configUnion = Clone(configuration);
                else
                    MergeConfig(configUnion, configuration);


            }
            return configUnion;
        }

        private static OpenIdConnectConfiguration Clone(OpenIdConnectConfiguration configuration)
        {
            var signingKeys = new List<SecurityKey>(configuration.SigningKeys);
            configuration.SigningKeys.Clear();


            var keySet = configuration.JsonWebKeySet;
            configuration.JsonWebKeySet = null;


            var json = OpenIdConnectConfiguration.Write(configuration);
            var clone = OpenIdConnectConfiguration.Create(json);


            foreach (var key in signingKeys)
            {
                configuration.SigningKeys.Add(key);
                clone.SigningKeys.Add(key);
            }


            configuration.JsonWebKeySet = keySet;
            clone.JsonWebKeySet = keySet;


            return clone;
        }


        private static void MergeConfig(OpenIdConnectConfiguration result, OpenIdConnectConfiguration source)
        {
            foreach (var alg in source.IdTokenSigningAlgValuesSupported)
            {
                if (!result.IdTokenSigningAlgValuesSupported.Contains(alg))
                {
                    result.IdTokenSigningAlgValuesSupported.Add(alg);
                }
            }


            foreach (var type in source.ResponseTypesSupported)
            {
                if (!result.ResponseTypesSupported.Contains(type))
                {
                    result.ResponseTypesSupported.Add(type);
                }
            }


            foreach (var type in source.SubjectTypesSupported)
            {
                if (!result.ResponseTypesSupported.Contains(type))
                {
                    result.SubjectTypesSupported.Add(type);
                }
            }


            foreach (var key in source.SigningKeys)
            {
                if (result.SigningKeys.All(k => k.KeyId != key.KeyId))
                {
                    result.SigningKeys.Add(key);
                }
            }
        }

        public Task<OpenIdConnectConfiguration> GetConfigurationByPolicyAsync(CancellationToken cancel, string policy)
        {
            if (string.IsNullOrEmpty(policy))
                throw new ArgumentNullException(nameof(policy));

            var policyId = policy.ToLowerInvariant();
            IConfigurationManager<OpenIdConnectConfiguration> configManager;
            if (_policyConfigurationManagers.TryGetValue(policyId, out configManager))
            {
                return configManager.GetConfigurationAsync(cancel);
            }

            throw new InvalidOperationException($"Invalid policy: {policy}");
        }

        public void RequestRefresh(string policy)
        {
            if (string.IsNullOrEmpty(policy))
                throw new ArgumentNullException(nameof(policy));

            var policyId = policy.ToLowerInvariant();
            IConfigurationManager<OpenIdConnectConfiguration> configManager;
            if (_policyConfigurationManagers.TryGetValue(policyId, out configManager))
            {
                configManager.RequestRefresh();
            }

            throw new InvalidOperationException($"Invalid policy: {policy}");

        }

        public void RequestRefresh()
        {
            foreach (KeyValuePair<string, IConfigurationManager<OpenIdConnectConfiguration>> configManager in _policyConfigurationManagers)
            {
                configManager.Value.RequestRefresh();
            }
        }
    }
}
