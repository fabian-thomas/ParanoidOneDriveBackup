using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using ParanoidOneDriveBackup.App;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ParanoidOneDriveBackup
{
    public class TokenCacheHelper<T> // taken from https://docs.microsoft.com/de-de/azure/active-directory/develop/msal-net-token-cache-serialization
    {

        private ILogger<T> _logger;
        public readonly string CacheFilePath = Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
        private readonly object FileLock = new object();

        public TokenCacheHelper(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                try
                {
                    args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                        ? AppData.Protector.Unprotect(File.ReadAllBytes(CacheFilePath))
                        : null);
                }
                catch (CryptographicException)
                {
                    _logger.LogWarning("MSAL token cache is invalid. Removing cache file. You need to reauthenticate.");
                }
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(CacheFilePath,
                                        AppData.Protector.Protect(args.TokenCache.SerializeMsalV3()));
                }
            }
        }
    }
}
