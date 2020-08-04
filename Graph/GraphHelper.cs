using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParanoidOneDriveBackup
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
            //graphClient.BaseUrl = "https://graph.microsoft.com/beta";
        }

        public static async Task<User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        public static async Task Test()
        {
            try
            {
                var file = "saved.csv";
                var children = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
                var id = children.Where(x => x.Name.Equals(file)).First().Id;

                var stream = await graphClient.Me.Drive.Items[id].Content.Request().GetAsync();

                var fileStream = System.IO.File.Create("C:\\Users\\fabit\\Desktop\\Test3\\"+ file);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Close();


                // GET /me
                //return await graphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                //return null;
            }
        }


        public static async Task<Drive> GetOneDriveAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Drive.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting OneDrive data: {ex.Message}");
                return null;
            }
        }

        public static async Task<IEnumerable<DriveItem>> GetDriveContentsAsync()
        {
            try
            {
                return await graphClient.Me.Drive.Root.Children.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting One Drive contents: {ex.Message}");
                return null;
            }
        }
    }
}