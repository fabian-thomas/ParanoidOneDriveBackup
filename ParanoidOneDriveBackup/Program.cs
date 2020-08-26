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
                    if (!File.Exists(Constants.CONFIG_FILE_PATH))
                    {
                        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
                        Directory.CreateDirectory(Constants.APP_DATA_FOLDER_PATH);
                        
                        // copy config file
                        using (var reader = embeddedProvider.GetFileInfo(Constants.CONFIG_FILE_NAME).CreateReadStream())
                        {
                            var fileStream = File.Create(Constants.CONFIG_FILE_PATH);
                            reader.Seek(0, SeekOrigin.Begin);
                            reader.CopyTo(fileStream);
                            fileStream.Close();
                        }
                        
                        if (!File.Exists(Constants.IGNORE_FILE_PATH))
                        {
                            // copy ignore file
                            using var reader = embeddedProvider.GetFileInfo(Constants.IGNORE_FILE_NAME).CreateReadStream();
                            var fileStream = File.Create(Constants.IGNORE_FILE_PATH);
                            reader.Seek(0, SeekOrigin.Begin);
                            reader.CopyTo(fileStream);
                            fileStream.Close();
                        }

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
