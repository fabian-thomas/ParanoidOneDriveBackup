using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ParanoidOneDriveBackup
{
    static class AppData
    {
        public static MsGraphConfig MsGraphConfig = new MsGraphConfig();
        public static IHostApplicationLifetime Lifetime;
        public static ILogger<Worker> Logger;

        public static void BindConfig(IConfiguration config)
        {
            ConfigurationBinder.Bind(config, Helper.GetDescription(typeof(MsGraphConfig)), MsGraphConfig);


            // TODO check if values in config are correct
        }
    }
}
