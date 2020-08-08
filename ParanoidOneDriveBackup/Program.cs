using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParanoidOneDriveBackup.App;
using System.IO;
using System.Reflection;

namespace ParanoidOneDriveBackup
{
    class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                          .AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                          .AddUserSecrets<Program>(optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    AppData.Protector = services.AddDataProtection().Services
                                                .BuildServiceProvider()
                                                .GetDataProtectionProvider()
                                                .CreateProtector("ParanoidOneDriveBackup"); // TODO move to startup and change implementation
                    services.AddHostedService<BackupService>();
                });
    }
}
