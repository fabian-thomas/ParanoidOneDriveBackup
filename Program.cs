using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    config.AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                          .AddUserSecrets<Program>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<BackupService>();
                });

        //static string FormatDriveInfo(Drive drive)
        //{
        //    var str = new StringBuilder();
        //    str.AppendLine($"The OneDrive Name is: {drive.Name}");
        //    str.AppendLine($"The OneDrive Ownder is: {drive.Owner.User.DisplayName}");
        //    str.AppendLine($"The OneDrive id is: {drive.Id}");
        //    str.AppendLine($"The OneDrive was modified last by: {drive?.LastModifiedBy?.User?.DisplayName}");

        //    return str.ToString();
        //}


        //static string ListOneDriveContents(List<DriveItem> contents)
        //{
        //    if (contents == null || contents.Count == 0)
        //    {
        //        return "No content found";
        //    }

        //    var str = new StringBuilder();
        //    foreach (var item in contents)
        //    {
        //        if (item.Folder != null)
        //        {
        //            str.AppendLine($"'{item.Name}' is a folder");
        //        }
        //        else if (item.File != null)
        //        {
        //            str.AppendLine($"'{item.Name}' is a file with size {item.Size}");
        //        }
        //        else if (item.Audio != null)
        //        {
        //            str.AppendLine($"'{item.Audio.Title}' is an audio file with size {item.Size}");
        //        }
        //        else
        //        {
        //            str.AppendLine($"Generic drive item found with name {item.Name}");
        //        }
        //    }

        //    return str.ToString();
        //}
    }
}
