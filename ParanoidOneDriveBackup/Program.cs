using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParanoidOneDriveBackup.App;
using System;
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
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        if (!File.Exists(Constants.CONFIG_FILE_PATH))
                        {
                            Console.WriteLine(Constants.CONFIG_FILE_PATH);
                            Console.WriteLine("--------------------------------------------config files does not exist");
                            // TODO directory does not exist
                            // TODO copy config file
                            //File.Copy(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        }
                        config.AddJsonFile(Constants.CONFIG_FILE_PATH);
                    }
                    else
                        config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                              .AddJsonFile("appsettings.json")
                              .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                              .AddUserSecrets<Program>(optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    AppData.Protector = DataProtectionProvider.Create("ParanoidOneDriveBackup").CreateProtector("ParanoidOneDriveBackup");// TODO move to startup and change implementation
                    //AppData.Protector = services.AddDataProtection().Services
                    //                            .BuildServiceProvider()
                    //                            .GetDataProtectionProvider()
                    //                            .CreateProtector("ParanoidOneDriveBackup"); 
                    services.AddHostedService<BackupService>();
                });
    }
}
