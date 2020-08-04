using Microsoft.Extensions.Configuration;

namespace ParanoidOneDriveBackup
{
    static class Config
    {
        public static MsGraphConfig MsGraph = new MsGraphConfig();

        public static void LoadConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddJsonFile("config.json")
                .Build();

            ConfigurationBinder.Bind(configuration, "MsGraph", MsGraph);


            // TODO check if values in config are correct
        }
    }
}
