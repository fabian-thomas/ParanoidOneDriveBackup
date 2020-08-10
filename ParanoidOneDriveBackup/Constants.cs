using System;
using System.IO;

namespace ParanoidOneDriveBackup
{
    static class Constants
    {
        public const string APP_TITLE = "ParanoidOneDriveBackup";

        public const string CONFIG_FILE_NAME = "config.json";

        public const string IGNORE_FILE_NAME = "ignore";

        public const string BACKUP_DIR_PREFIX = "OneDrive_";

        /*
         * Linux: home/<user>/.config/apptitle
         * Windows: %localappdata%/apptitle
         */
        public static string APP_DATA_FOLDER_PATH
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_TITLE);
                else
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_TITLE);
            }
        }

        public static string CONFIG_FILE_PATH
        {
            get => Path.Combine(APP_DATA_FOLDER_PATH, CONFIG_FILE_NAME);
        }

        public static string IGNORE_FILE_PATH
        {
            get => Path.Combine(APP_DATA_FOLDER_PATH, IGNORE_FILE_NAME);
        }

        public const string TOKEN_CACHE_FILE_NAME = "token_cache.bin3";

        /*
         * Linux: home/<user>/.cache/apptitle
         * Windows: %localappdata%/apptitle
         */
        public static string CACHE_FOLDER_PATH
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".cache", APP_TITLE);
                else
                    return APP_DATA_FOLDER_PATH;
            }
        }

        public static string TOKEN_CACHE_FILE_PATH
        {
            get => Path.Combine(CACHE_FOLDER_PATH, TOKEN_CACHE_FILE_NAME);
        }
    }
}
