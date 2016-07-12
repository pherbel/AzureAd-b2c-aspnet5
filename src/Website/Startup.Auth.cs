using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Website.Services;
using System.Threading;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Website
{
    public partial class Startup
    {
        public void ConfigureAuthentication(IApplicationBuilder app, IOptions<AzureAdSettings> azureADSettings, IServiceProvider serviceProvider)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = AuthenticationConstants.OpenIdConnectAzureAdB2CAuthenticationScheme,
                AutomaticChallenge = true,
                ClientId = azureADSettings.Value.ClientId,
                Authority = azureADSettings.Value.Authority,
                ResponseType = OpenIdConnectResponseType.IdToken,
                PostLogoutRedirectUri = azureADSettings.Value.PostLogoutRedirectUri,
                Events = new OpenIdConnectEvents
                {
                    OnAuthenticationFailed = OnAuthenticationFailed,
                    //  OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
                },

                // The PolicyConfigurationManager takes care of getting the correct Azure AD authentication 
                // endpoints from the OpenID Connect metadata endpoint.  It is included in the Authentication folder. 
                ConfigurationManager = serviceProvider.GetRequiredService<PolicyConfigurationManager>(),
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme

            });
        }
        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }



        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            AzureAdSettings azureADSettings = GetAdSettings(context);


            context.HandleCodeRedemption();

        }

        private async Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            AzureAdSettings azureADSettings = GetAdSettings(context);
            var configuration = await GetOpenIdConnectConfigurationAsync(context, azureADSettings.B2CPolicySettings.SignInOrSignUpPolicy);
            context.ProtocolMessage.IssuerAddress = configuration.AuthorizationEndpoint;

        }

        private async Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            AzureAdSettings azureADSettings = GetAdSettings(context);
            var configuration = await GetOpenIdConnectConfigurationAsync(context, azureADSettings.B2CPolicySettings.SignInOrSignUpPolicy);
            context.ProtocolMessage.IssuerAddress = configuration.EndSessionEndpoint;

        }
        private AzureAdSettings GetAdSettings(BaseOpenIdConnectContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<IOptions<AzureAdSettings>>().Value;
        }
        private static async Task<OpenIdConnectConfiguration> GetOpenIdConnectConfigurationAsync(RedirectContext context, string defaultPolicy)
        {
            var manager = (PolicyConfigurationManager)context.Options.ConfigurationManager;
            var policy = context.Properties.Items.ContainsKey(AuthenticationConstants.B2CPolicy) ? context.Properties.Items[AuthenticationConstants.B2CPolicy] : defaultPolicy;
            var configuration = await manager.GetConfigurationByPolicyAsync(CancellationToken.None, policy);
            return configuration;
        }

    }
}
