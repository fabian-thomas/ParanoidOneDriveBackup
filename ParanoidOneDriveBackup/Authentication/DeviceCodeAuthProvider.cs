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
        private string _clientId;
        private TokenCacheHelper<T> _tokenCacheHelper;

        public DeviceCodeAuthProvider(string clientId, string[] scopes, ILogger<T> logger, TokenCacheHelper<T> tokenCacheHelper)
        {
            _scopes = scopes;
            _logger = logger;
            _clientId = clientId;
            _tokenCacheHelper = tokenCacheHelper;
        }

        public async Task<bool> InitializeAuthentication()
        {
            try
            {
                _authClient = PublicClientApplicationBuilder.Create(_clientId)
                                                            .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
                                                            .Build();

                _tokenCacheHelper.EnableSerialization(_authClient.UserTokenCache);

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
            catch (MsalClientException ex)
            {
                if (ex.ErrorCode != null && ex.ErrorCode.Equals("client_id_must_be_guid"))
                    _logger.LogCritical("You have to specify a valid MS graph client id. Was \"{0}\"", _clientId);
                else
                    _logger.LogCritical("Can't authenticate. Try deleting the cache file \"{1}\".\n{0}", ex, App.Constants.TOKEN_CACHE_FILE_PATH);
            }
            return false;
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