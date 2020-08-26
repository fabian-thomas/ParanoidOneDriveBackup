using System;
using System.IO;

namespace ParanoidOneDriveBackup.App
{
    internal static class Constants
    {
        public const string AppTitle = "ParanoidOneDriveBackup";

        public const string ConfigFileName = "config.json";

        public const string IgnoreFileName = "ignore";

        private const string TokenCacheFileName = "token_cache.bin3";

        public const string BackupDirPrefix = "OneDrive_";

        /*
         * Linux: home/<user>/.config/ParanoidOneDriveBackup
         * Windows: %localappdata%/ParanoidOneDriveBackup
         */
        public static string AppDataFolderPath =>
            Path.Combine(
                Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppTitle);

        public static string ConfigFilePath => Path.Combine(AppDataFolderPath, ConfigFileName);

        public static string IgnoreFilePath => Path.Combine(AppDataFolderPath, IgnoreFileName);

        /*
         * Linux: home/<user>/.cache/ParanoidOneDriveBackup
         * Windows: %localappdata%/ParanoidOneDriveBackup
         */
        private static string CacheFolderPath
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".cache",
                        AppTitle);
                else
                    return AppDataFolderPath;
            }
        }

        public static string TokenCacheFilePath => Path.Combine(CacheFolderPath, TokenCacheFileName);
    }
}