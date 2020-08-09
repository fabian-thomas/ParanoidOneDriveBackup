namespace ParanoidOneDriveBackup
{
    static class Constants
    {
        public const string BACKUP_DIR_PREFIX = "OneDrive_";

        public static string CONFIG_FOLDER_PATH
        {
            get => $@"{Helper.GetApplicationDataFolderPath()}/ParanoidOneDriveBackup";
        }

        public static string CONFIG_FILE_PATH
        {
            get => $@"{CONFIG_FOLDER_PATH}/appsettings.json";
        }

        public static string TOKEN_CACHE_FILE_PATH
        {
            get => $@"{CONFIG_FOLDER_PATH}/token_cache.bin3";
        }
    }
}
