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
                    if (!File.Exists(Constants.CONFIG_FILE_PATH))
                    {
                        File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", Constants.CONFIG_FILE_NAME), Constants.CONFIG_FILE_PATH);
                        if (!File.Exists(Constants.IGNORE_FILE_PATH))
                            File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", Constants.IGNORE_FILE_NAME), Constants.IGNORE_FILE_PATH);
                        Console.WriteLine($"No config file found. The default config file has been copied to \"{Constants.CONFIG_FILE_PATH}\". Modify it and restart the application.");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                    config.AddJsonFile(Constants.CONFIG_FILE_PATH);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    AppData.Protector = DataProtectionProvider.Create(Constants.APP_TITLE).CreateProtector(Constants.APP_TITLE);// TODO move to startup and change implementation
                    //AppData.Protector = services.AddDataProtection().Services
                    //                            .BuildServiceProvider()
                    //                            .GetDataProtectionProvider()
                    //                            .CreateProtector("ParanoidOneDriveBackup"); 
                    services.AddHostedService<BackupService>();
                });
    }
}
