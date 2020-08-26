using MAB.DotIgnore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ParanoidOneDriveBackup.App.Configuration;

namespace ParanoidOneDriveBackup.App
{
    internal static class AppData
    {
        public static readonly MsGraphConfig MsGraphConfig = new MsGraphConfig();
        public static readonly BackupConfig BackupConfig = new BackupConfig();
        public static IHostApplicationLifetime Lifetime;
        public static IDataProtector Protector;
        public static IgnoreList Ignore;

        public static void BindConfig(IConfiguration config)
        {
            config.Bind(Helper.GetDescription(typeof(MsGraphConfig)), MsGraphConfig);
            config.Bind(Helper.GetDescription(typeof(BackupConfig)), BackupConfig);


            // TODO check if values in config are correct
        }
    }
}
