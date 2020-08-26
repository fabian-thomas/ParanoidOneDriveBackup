using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParanoidOneDriveBackup.App;
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace ParanoidOneDriveBackup
{
    internal static class Program 
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("Version: {0}", Helper.GetAppVersion());
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (!File.Exists(Constants.ConfigFilePath))
                    {
                        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
                        Directory.CreateDirectory(Constants.AppDataFolderPath);
                        
                        // copy config file
                        using (var reader = embeddedProvider.GetFileInfo(Constants.ConfigFileName).CreateReadStream())
                        {
                            var fileStream = File.Create(Constants.ConfigFilePath);
                            reader.Seek(0, SeekOrigin.Begin);
                            reader.CopyTo(fileStream);
                            fileStream.Close();
                        }
                        
                        if (!File.Exists(Constants.IgnoreFilePath))
                        {
                            // copy ignore file
                            using var reader = embeddedProvider.GetFileInfo(Constants.IgnoreFileName).CreateReadStream();
                            var fileStream = File.Create(Constants.IgnoreFilePath);
                            reader.Seek(0, SeekOrigin.Begin);
                            reader.CopyTo(fileStream);
                            fileStream.Close();
                        }

                        Console.WriteLine($"No config file found. The default config file has been copied to \"{Constants.ConfigFilePath}\". Modify it and restart the application.");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                    config.AddJsonFile(Constants.ConfigFilePath);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    AppData.Protector = DataProtectionProvider.Create(Constants.AppTitle).CreateProtector(Constants.AppTitle);// TODO move to startup and change implementation
                    //AppData.Protector = services.AddDataProtection().Services
                    //                            .BuildServiceProvider()
                    //                            .GetDataProtectionProvider()
                    //                            .CreateProtector("ParanoidOneDriveBackup"); 
                    services.AddHostedService<BackupService>();
                });
    }
}
