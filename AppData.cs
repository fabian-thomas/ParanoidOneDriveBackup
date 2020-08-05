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

        public static void LoadConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddJsonFile("config.json")
                .Build();

            ConfigurationBinder.Bind(configuration, "MsGraph", MsGraphConfig);


            // TODO check if values in config are correct
        }
    }
}
