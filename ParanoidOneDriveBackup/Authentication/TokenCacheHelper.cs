using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using ParanoidOneDriveBackup.App;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ParanoidOneDriveBackup
{
    public class TokenCacheHelper<T> // taken from https://docs.microsoft.com/de-de/azure/active-directory/develop/msal-net-token-cache-serialization
    {

        private readonly ILogger<T> _logger;
        private readonly string _cacheFilePath;
        private readonly object _fileLock = new object();

        public TokenCacheHelper(ILogger<T> logger, string cacheFilePath)
        {
            _logger = logger;
            _cacheFilePath = cacheFilePath;
        }

        public void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_fileLock)
            {
                try
                {
                    args.TokenCache.DeserializeMsalV3(File.Exists(_cacheFilePath)
                        ? AppData.Protector.Unprotect(File.ReadAllBytes(_cacheFilePath))
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
                lock (_fileLock)
                {
                    // reflect changes in the persistent store
                    Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath));
                    File.WriteAllBytes(_cacheFilePath,
                                        AppData.Protector.Protect(args.TokenCache.SerializeMsalV3()));
                }
            }
        }
    }
}
