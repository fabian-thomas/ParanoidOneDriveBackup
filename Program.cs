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
        protected static IConfigurationRoot Configuration;

        static async Task Main(string[] args)
        {
            LoadConfiguration();

            var authProvider = new DeviceCodeAuthProvider(Configuration[C.MSGRAPH_ID], Helper.ScopesStringToArray(Configuration[C.MSGRAPH_SCOPES]));
            await authProvider.InitializeAuthentication();

            GraphHelper.Initialize(authProvider);

            await GraphHelper.Test();

            Console.ReadKey();
        }

        private static void LoadConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json")
                .Build();


            if (string.IsNullOrEmpty(Configuration[C.MSGRAPH_ID]) || string.IsNullOrEmpty(Configuration[C.MSGRAPH_SCOPES]))
            {
                // TODO
                throw new Exception();
            }
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
