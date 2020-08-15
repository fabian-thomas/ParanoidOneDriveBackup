using MAB.DotIgnore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParanoidOneDriveBackup.App.Configuration;

namespace ParanoidOneDriveBackup.App
{
    static class AppData
    {
        public static MsGraphConfig MsGraphConfig = new MsGraphConfig();
        public static BackupConfig BackupConfig = new BackupConfig();
        public static ExportImportConfig ExportImportConfig = new ExportImportConfig();
        public static IHostApplicationLifetime Lifetime;
        public static IDataProtector Protector;
        public static IgnoreList Ignore;

        public static void BindConfig(IConfiguration config)
        {
            ConfigurationBinder.Bind(config, Helper.GetDescription(typeof(MsGraphConfig)), MsGraphConfig);
            ConfigurationBinder.Bind(config, Helper.GetDescription(typeof(BackupConfig)), BackupConfig);
            ConfigurationBinder.Bind(config, Helper.GetDescription(typeof(ExportImportConfig)), ExportImportConfig);

            // TODO check if values in config are correct
        }
    }
}
