using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ParanoidOneDriveBackup
{
    public class DeviceCodeAuthProvider<T> : IAuthenticationProvider
    {
        private IPublicClientApplication _authClient;
        private string[] _scopes;
        private IAccount _userAccount;
        private ILogger<T> _logger;

        public DeviceCodeAuthProvider(string appId, string[] scopes, ILogger<T> logger)
        {
            _scopes = scopes;
            _logger = logger;

            _authClient = PublicClientApplicationBuilder
                .Create(appId)
                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
                .Build();

            TokenCacheHelper.EnableSerialization(_authClient.UserTokenCache);
        }

        public async Task<bool> InitializeAuthentication()
        {
            // check if there is an account in cache
            var accounts = await _authClient.GetAccountsAsync();
            _userAccount = accounts.FirstOrDefault();

            if (_userAccount == null)
            {
                try
                {
                    // acquire token over device login
                    var result = await _authClient.AcquireTokenWithDeviceCode(_scopes, callback =>
                    {
                        Console.WriteLine(callback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();

                    _userAccount = result.Account;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Error during authentication.\n{0}", ex);
                    return false;
                }
            }
            return true;
        }

        public async Task<string> GetAccessToken()
        {
            var result = await _authClient.AcquireTokenSilent(_scopes, _userAccount)
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