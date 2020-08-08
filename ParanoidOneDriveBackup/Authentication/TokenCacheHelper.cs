using Microsoft.Identity.Client;
using ParanoidOneDriveBackup.App;
using System.IO;
using System.Reflection;

namespace ParanoidOneDriveBackup
{
    static class TokenCacheHelper // taken from https://docs.microsoft.com/de-de/azure/active-directory/develop/msal-net-token-cache-serialization
    {

        public static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }


        public static readonly string CacheFilePath = Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";

        private static readonly object FileLock = new object();


        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                        ? AppData.Protector.Unprotect(File.ReadAllBytes(CacheFilePath))
                        : null);
            }
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changesgs in the persistent store
                    File.WriteAllBytes(CacheFilePath,
                                        AppData.Protector.Protect(args.TokenCache.SerializeMsalV3()));
                }
            }
        }
    }
}
