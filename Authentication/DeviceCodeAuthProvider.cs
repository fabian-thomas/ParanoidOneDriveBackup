using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ParanoidOneDriveBackup
{
    public class DeviceCodeAuthProvider : IAuthenticationProvider
    {
        private IPublicClientApplication authClient;
        private string[] scopes;
        private IAccount userAccount;

        public DeviceCodeAuthProvider(string appId, string[] scopes)
        {
            this.scopes = scopes;

            authClient = PublicClientApplicationBuilder
                .Create(appId)
                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
                .Build();

            TokenCacheHelper.EnableSerialization(authClient.UserTokenCache);
        }

        public async Task InitializeAuthentication()
        {
            // check if there is an account in cache
            var accounts = await authClient.GetAccountsAsync();
            userAccount = accounts.FirstOrDefault();

            if (userAccount == null)
            {
                try
                {
                    // acquire token over device login
                    var result = await authClient.AcquireTokenWithDeviceCode(scopes, callback =>
                    {
                        return Task.FromResult(0);
                    }).ExecuteAsync();

                    userAccount = result.Account;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error during authentication: {exception.Message}");
                    // TODO logging
                }
            }
        }

        public async Task<string> GetAccessToken()
        {
            var result = await authClient
                .AcquireTokenSilent(scopes, userAccount)
                .ExecuteAsync();

            return result.AccessToken;

            // TODO what happens when token is rejeced during process running
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}