using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace ParanoidOneDriveBackup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Working with Graph and One Drive is fun!");

            var appConfig = LoadAppSettings();

            if (appConfig == null)
            {
                Console.WriteLine("Missing or invalid appsettings.json...exiting");
                return;
            }

            var appId = appConfig["appId"];
            var tenantId = "f8cdef31-a31e-4b4a-93e4-5f571e91255a";
            var scopesString = appConfig["scopes"];
            var scopes = scopesString.Split(';');
            //string[] scopes = new string[] { "email", "Files.Read.All", "offline_access", "openid", "profile", "User.Read" };//new string[] { "default" }; // "https://graph.microsoft.com/.default"

            // Initialize the auth provider with values from appsettings.json
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = authProvider.GetAccessToken().Result;
            GraphHelper.Initialize(authProvider);

            int choice = -1;
            while (choice != 0)
            {
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Display your access token");
                Console.WriteLine("2. Get your OneDrive root folder");
                Console.WriteLine("3. List your OneDrive contents");
                try
                {
                    choice = int.Parse(Console.ReadLine());
                }
                catch (System.FormatException)
                {
                    // Set to invalid value
                    choice = -1;
                }

                switch (choice)
                {
                    case 0:
                        // Exit the program
                        Console.WriteLine("Goodbye...");
                        break;
                    case 1:
                        // Display access token
                        Console.WriteLine($"The access token is:{accessToken}");
                        break;
                    case 2:
                        // Get OneDrive Info
                        Console.WriteLine(string.Empty);
                        Console.ForegroundColor = ConsoleColor.Green;
                        var driveInfo = await GraphHelper.GetOneDriveAsync();
                        //GraphHelper.GetDriveContentsAsync().Result.First().
                        Console.WriteLine(FormatDriveInfo(driveInfo));
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case 3:
                        // Get OneDrive contents
                        var driveContents = await GraphHelper.GetDriveContentsAsync();
                        Console.WriteLine(string.Empty);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(ListOneDriveContents(driveContents.ToList()));
                        Console.ForegroundColor = ConsoleColor.White;

                        break;

                    case 4:
                        GraphHelper.Test();
                        break;
                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        break;
                }
            }
        }

        static IConfigurationRoot LoadAppSettings()
        {
            var appConfig = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Check for required settings
            if (string.IsNullOrEmpty(appConfig["appId"]) || string.IsNullOrEmpty(appConfig["scopes"]))
            {
                return null;
            }

            return appConfig;
        }

        static string FormatDriveInfo(Drive drive)
        {
            var str = new StringBuilder();
            str.AppendLine($"The OneDrive Name is: {drive.Name}");
            str.AppendLine($"The OneDrive Ownder is: {drive.Owner.User.DisplayName}");
            str.AppendLine($"The OneDrive id is: {drive.Id}");
            str.AppendLine($"The OneDrive was modified last by: {drive?.LastModifiedBy?.User?.DisplayName}");

            return str.ToString();
        }


        static string ListOneDriveContents(List<DriveItem> contents)
        {
            if (contents == null || contents.Count == 0)
            {
                return "No content found";
            }

            var str = new StringBuilder();
            foreach (var item in contents)
            {
                if (item.Folder != null)
                {
                    str.AppendLine($"'{item.Name}' is a folder");
                }
                else if (item.File != null)
                {
                    str.AppendLine($"'{item.Name}' is a file with size {item.Size}");
                }
                else if (item.Audio != null)
                {
                    str.AppendLine($"'{item.Audio.Title}' is an audio file with size {item.Size}");
                }
                else
                {
                    str.AppendLine($"Generic drive item found with name {item.Name}");
                }
            }

            return str.ToString();
        }
    }
}
