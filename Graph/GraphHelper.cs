using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace ParanoidOneDriveBackup
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;

        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        //public static async Task<User> GetMeAsync()
        //{
        //    try
        //    {
        //        // GET /me
        //        return await client.Me.Request().GetAsync();
        //    }
        //    catch (ServiceException ex)
        //    {
        //        Console.WriteLine($"Error getting signed-in user: {ex.Message}");
        //        return null;
        //    }
        //}

        //public static async Task Test()
        //{
        //    try
        //    {
        //        var file = "saved.csv";
        //        var children = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
        //        var id = children.Where(x => x.Name.Equals(file)).First().Id;

        //        var stream = await graphClient.Me.Drive.Items[id].Content.Request().GetAsync();

        //        var fileStream = File.Create(Directory.GetCurrentDirectory() + "/" + file);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        stream.CopyTo(fileStream);
        //        fileStream.Close();


        //        // GET /me
        //        //return await graphClient.Me.Request().GetAsync();
        //    }
        //    catch (ServiceException ex)
        //    {
        //        Console.WriteLine($"Error getting signed-in user: {ex.Message}");
        //        //return null;
        //    }
        //}

        private static async Task DownloadAllRecursive(DriveItem item, string parentDirectoryPath)
        {
            // TODO loggin
            if (item.Folder != null)
            {
                // create directory and download all children recursively
                var childDirectoryPath = parentDirectoryPath + @"/" + item.Name;
                Directory.CreateDirectory(childDirectoryPath);

                var children = await graphClient.Me.Drive.Items[item.Id].Children.Request().GetAsync();

                foreach (var child in children)
                    await DownloadAllRecursive(child, childDirectoryPath);
            }
            else if (item.File != null)
            {
                // download single file
                var filePath = parentDirectoryPath + @"/" + item.Name;
                var fileStream = File.Create(filePath);

                var contentStream = await graphClient.Me.Drive.Items[item.Id].Content
                                .Request()
                                .GetAsync();

                contentStream.Seek(0, SeekOrigin.Begin);
                contentStream.CopyTo(fileStream);
                fileStream.Close();
            }
            else if (item.Package != null && item.Package.Type.Equals("oneNote"))
            {
                return;
                // onenote notebook

                // TODO
                //throw new Exception();


                var ite = graphClient.Me.Drive.Items[item.Id];
                var c = ite.Content;
                var r = c.Request();
                //graphClient.Me.Onenote.
                //var stream3 = await r
                //                .GetAsync();


                try
                {
                    var filePath = parentDirectoryPath + @"/TestPage.one";
                    //var filePath = parentDirectoryPath + @"/" + item.Name;
                    var fileStream = File.Create(filePath);

                    //var stream = await graphClient.Me.Drive.Items[item.Id].Content
                    //                .Request()
                    //                .GetAsync();



                    var notebooks = await graphClient.Me.Onenote.Notebooks.Request().GetAsync();
                    var note = notebooks.First();
                    Microsoft.Graph.Notebook notebook = await graphClient.Me.Onenote.Notebooks[note.Id].Request().GetAsync();

                    var pags = await graphClient.Me.Onenote.Pages.Request().Filter("title eq 'TestPage'").GetAsync();

                    var stream = await graphClient.Me.Onenote.Pages[pags.First().Id].Content.Request().GetAsync();


                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Close();

                    //var result = await graphClient.Me.Onenote.Pages.Request().GetAsync();
                    //foreach (var page in result)
                    //{

                    //    //download Page content
                    //    var message = new HttpRequestMessage(HttpMethod.Get, page.ContentUrl);
                    //    await graphClient.AuthenticationProvider.AuthenticateRequestAsync(message);
                    //    var response = await graphClient.HttpProvider.SendAsync(message);
                    //    var content = await response.Content.ReadAsStringAsync();  //get content as HTML 

                    //}


                    var res = await graphClient.Me.Onenote.Resources.Request().GetAsync();

                    stream = await graphClient.Me.Onenote.Resources[pags.First().Id].Content
                                    .Request()
                                    .GetAsync();


                    filePath = parentDirectoryPath + @"/TestPage2.one";
                    fileStream = File.Create(filePath);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Close();

                    //var stream = await graphClient.Me.Drive.Items[id].Content.Request().GetAsync();

                    //var fileStream = File.Create(Directory.GetCurrentDirectory() + "/" + file);

                }
                catch (ServiceException ex)
                {

                }

            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static async Task DownloadAll(string path)
        {
            try
            {
                await DownloadAllRecursive(graphClient.Me.Drive.Root.Request().GetAsync().Result, path);
            }
            catch (ServiceException ex)
            {
                // TODO
            }
        }


        //public static async Task<Drive> GetOneDriveAsync()
        //{
        //    try
        //    {
        //        // GET /me
        //        return await client.Me.Drive.Request().GetAsync();
        //    }
        //    catch (ServiceException ex)
        //    {
        //        Console.WriteLine($"Error getting OneDrive data: {ex.Message}");
        //        return null;
        //    }
        //}

        //public static async Task<IEnumerable<DriveItem>> GetDriveContentsAsync()
        //{
        //    try
        //    {
        //        return await client.Me.Drive.Root.Children.Request().GetAsync();
        //    }
        //    catch (ServiceException ex)
        //    {
        //        Console.WriteLine($"Error getting One Drive contents: {ex.Message}");
        //        return null;
        //    }
        //}
    }
}