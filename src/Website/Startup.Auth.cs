using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.OptionsModel;
using Website.Properties;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Threading;
using Website.PolicyAuthHelpers;

namespace Website
{
    public partial class Startup
    {
        // The ACR claim is used to indicate which policy was executed 
        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";
        public const string PolicyKey = "b2cpolicy";

        public void ConfigureAuth(IApplicationBuilder app, IOptions<AzureADSettings> azureADSettings)
        {
            // Configure the OWIN Pipeline to use Cookie Authentication 
            app.UseCookieAuthentication(options =>
            {
                // By default, all middleware are passive/not automatic. Making cookie middleware automatic so that it acts on all the messages. 
                options.AutomaticAuthenticate = true;


            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.ClientId = azureADSettings.Value.ClientId;
                options.ResponseType = OpenIdConnectResponseTypes.IdToken;
                options.Authority = string.Format(CultureInfo.InvariantCulture, azureADSettings.Value.AadInstance, azureADSettings.Value.Tenant,string.Empty,string.Empty);
                options.Events = new OpenIdConnectEvents {
                    OnAuthenticationFailed =   OnAuthenticationFailed,
                    OnRedirectToAuthenticationEndpoint = OnRedirectToAuthenticationEndpoint
                };

                // The PolicyConfigurationManager takes care of getting the correct Azure AD authentication 
                // endpoints from the OpenID Connect metadata endpoint.  It is included in the PolicyAuthHelpers folder. 
                options.ConfigurationManager = new PolicyConfigurationManager(
                    String.Format(CultureInfo.InvariantCulture, azureADSettings.Value.AadInstance, azureADSettings.Value.Tenant, "/v2.0","/" + OpenIdProviderMetadataNames.Discovery),
                    new string[] { azureADSettings.Value.SignUpPolicyId, azureADSettings.Value.SignInPolicyId, azureADSettings.Value.UserProfilePolicyId});

            });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }


        // This notification can be used to manipulate the OIDC request before it is sent.  Here we use it to send the correct policy. 
        private async Task OnRedirectToAuthenticationEndpoint(RedirectContext context)
        {
            IOptions<AzureADSettings> azureADSettings = (IOptions<AzureADSettings>)context.HttpContext.ApplicationServices.GetService(typeof(IOptions<AzureADSettings>));
            PolicyConfigurationManager mgr = context.Options.ConfigurationManager as PolicyConfigurationManager;
            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            {
                if (context.Request.Path.Value.ToLower().Contains("signup"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.SignUpPolicyId);
                    context.ProtocolMessage.IssuerAddress = config.EndSessionEndpoint;
                }
                else if (context.Request.Path.Value.ToLower().Contains("signin"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.SignInPolicyId);
                    context.ProtocolMessage.IssuerAddress = config.EndSessionEndpoint;
                }
                else if (context.Request.Path.Value.ToLower().Contains("profile"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.UserProfilePolicyId);
                    context.ProtocolMessage.IssuerAddress = config.EndSessionEndpoint;
                }
            }
            else
            {
                if (context.Request.Path.Value.ToLower().Contains("signup"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.SignUpPolicyId);
                    context.ProtocolMessage.IssuerAddress = config.AuthorizationEndpoint;
                }
                else if (context.Request.Path.Value.ToLower().Contains("signin"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.SignInPolicyId);
                    context.ProtocolMessage.IssuerAddress = config.AuthorizationEndpoint;
                }
                else if (context.Request.Path.Value.ToLower().Contains("profile"))
                {
                    OpenIdConnectConfiguration config = await mgr.GetConfigurationByPolicyAsync(CancellationToken.None, azureADSettings.Value.UserProfilePolicyId);
                    context.ProtocolMessage.IssuerAddress = config.AuthorizationEndpoint;
                }
            }


        }

    }
}
