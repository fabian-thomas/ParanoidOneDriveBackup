using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using ParanoidOneDriveBackup.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace ParanoidOneDriveBackup
{
    public class GraphHelper<T>
    {
        private GraphServiceClient _graphClient;
        private ILogger<T> _logger;
        private List<Task> downloadTasks;
        private CancellationToken _ct;
        private string _rootPath;
        private int _maxParallelDownloadTasks;
        private decimal _reportingSteps;
        private bool _reportingEnabled;

        public GraphHelper(ILogger<T> logger, IAuthenticationProvider authProvider, CancellationToken ct, string rootPath, int maxParallelDownloadTasks, decimal reportingSteps, bool reportingEnabled)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _logger = logger;
            _ct = ct;
            _rootPath = rootPath;
            _maxParallelDownloadTasks = maxParallelDownloadTasks;
            _reportingSteps = reportingSteps;
            _reportingEnabled = reportingEnabled;
        }

        private async Task DownloadAllRecursive(DriveItem item, string parentDirectoryRelativePath)
        {
            var childRelativePath = Path.Combine(parentDirectoryRelativePath, item.Name);

            if (item.Folder != null)
            {
                // check ignore
                if (AppData.Ignore.IsIgnored(childRelativePath, true))
                {
                    _logger.LogDebug("Ignoring folder: \"{0}\"", childRelativePath);
                    return;
                }

                // create directory and download all children recursively
                if (item.Root != null)
                {
                    // item is root item
                    childRelativePath = "";
                }

                try
                {
                    if (!_ct.IsCancellationRequested)
                    {
                        var children = await _graphClient.Me.Drive.Items[item.Id].Children.Request().GetAsync();

                        if (item.Root != null)
                        {
                            // item is root item
                            try
                            {
                                Directory.CreateDirectory(_rootPath);
                                _logger.LogDebug("Created Directory: \"{0}\"", _rootPath);
                            }
                            catch (IOException ex)
                            {
                                _logger.LogError("Could not create directory \"{0}\". No items have been downloaded.\n{1}", _rootPath, ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(Path.Combine(_rootPath, childRelativePath));
                                _logger.LogDebug("Created Directory: \"{0}\"", childRelativePath);
                            }
                            catch (IOException ex)
                            {
                                _logger.LogError("Could not create directory \"{0}\". No children have been downloaded.\n{1}", childRelativePath, ex);
                            }
                        }

                        if (!_ct.IsCancellationRequested)
                            foreach (var child in children)
                            {
                                if (child.Folder != null)
                                {
                                    await DownloadAllRecursive(child, childRelativePath);
                                }
                                else
                                {
                                    if (_maxParallelDownloadTasks != 0)
                                    {
                                        downloadTasks.RemoveAll(x => x.IsCompleted);
                                        if (downloadTasks.Count >= _maxParallelDownloadTasks)
                                        {
                                            _logger.LogDebug("Maximum number of download tasks reached. Awating any task to finish.");
                                            var index = Task.WaitAny(downloadTasks.ToArray(), _ct);
                                            if (index >= 0 && index < downloadTasks.Count)
                                                downloadTasks.RemoveAt(index);
                                        }
                                    }
                                    downloadTasks.Add(DownloadAllRecursive(child, childRelativePath));
                                }
                            }
                    }
                }
                catch (ServiceException ex)
                {
                    _logger.LogError("Could not get children of directory \"{0}\".\n{1}", childRelativePath, ex);
                }
            }
            else if (item.File != null)
            {
                // check ignore
                if (AppData.Ignore.IsIgnored(childRelativePath, false))
                    _logger.LogDebug("Ignoring file: \"{0}\"", childRelativePath);
                else
                {
                    // download single file
                    try
                    {
                        if (!_ct.IsCancellationRequested)
                        {
                            var fileStream = File.Create(Path.Combine(_rootPath, childRelativePath));

                            var contentStream = await _graphClient.Me.Drive.Items[item.Id].Content.Request()
                                                                                                  .GetAsync();

                            if (!_ct.IsCancellationRequested)
                            {
                                _logger.LogDebug("Downloading file: \"{0}\"", childRelativePath);

                                contentStream.Seek(0, SeekOrigin.Begin);
                                contentStream.CopyTo(fileStream);
                                fileStream.Close();
                            }
                        }
                    }
                    catch (ServiceException ex)
                    {
                        _logger.LogError("Could not download file \"{0}\".\n{1}", childRelativePath, ex);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError("Could not create file \"{0}\".\n{1}", childRelativePath, ex);
                    }
                }

                // update progress
                try
                {
                    var driveItem = await _graphClient.Me.Drive.Items[item.Id].Request()
                                                                              .GetAsync();
                    if (driveItem.Size.HasValue)
                        ReportProgress(driveItem.Size.Value);
                    else
                        _logger.LogWarning("File \"{0}\" has no size. Progress may be inaccurate.", childRelativePath);
                }
                catch (ServiceException ex)
                {
                    _logger.LogWarning("Could not update progress of file \"{0}\".\n{1}", childRelativePath, ex);
                }
            }
            else if (item.Package != null && item.Package.Type.Equals("oneNote"))
            {
                // check ignore
                //if (AppData.Ignore.IsIgnored(childRelativePath, false))
                //{
                //    _logger.LogDebug("Ignored file: \"{0}\"", childRelativePath);
                //    return;
                //}
                var x = (await _graphClient.Me.Drive.Items[item.Id]
                                       .Request()
                                       .GetAsync());
                if (x.Size.HasValue)
                {
                    ReportProgress(x.Size.Value);
                }
                else
                    _logger.LogCritical("dfhjash");



                _logger.LogWarning("Onenote file \"{0}\"", childRelativePath);
                return;
                // onenote notebook

                // TODO
                //throw new Exception();


                var ite = _graphClient.Me.Drive.Items[item.Id];
                var c = ite.Content;
                var r = c.Request();
                //graphClient.Me.Onenote.
                //var stream3 = await r
                //                .GetAsync();


                try
                {
                    var filePath = parentDirectoryRelativePath + @"/TestPage.one";
                    //var filePath = parentDirectoryPath + @"/" + item.Name;
                    var fileStream = File.Create(filePath);

                    //var stream = await graphClient.Me.Drive.Items[item.Id].Content
                    //                .Request()
                    //                .GetAsync();



                    var notebooks = await _graphClient.Me.Onenote.Notebooks.Request().GetAsync();
                    var note = notebooks.First();
                    Microsoft.Graph.Notebook notebook = await _graphClient.Me.Onenote.Notebooks[note.Id].Request().GetAsync();

                    var pags = await _graphClient.Me.Onenote.Pages.Request().Filter("title eq 'TestPage'").GetAsync();

                    var stream = await _graphClient.Me.Onenote.Pages[pags.First().Id].Content.Request().GetAsync();


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


                    var res = await _graphClient.Me.Onenote.Resources.Request().GetAsync();

                    stream = await _graphClient.Me.Onenote.Resources[pags.First().Id].Content
                                    .Request()
                                                                                        .GetAsync();


                    filePath = parentDirectoryRelativePath + @"/TestPage2.one";
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
                if (item.Package == null)
                    _logger.LogError("File \"{0}\" is not supported.", childRelativePath);
                else
                    _logger.LogError("File \"{1}\" of type {0} is not supported.", item.Package.Type, childRelativePath);

            }
        }

        private long _downloaded;
        private long _total;
        private decimal _lastReported;

        public async Task DownloadAll()
        {
            try
            {
                _logger.LogInformation("Backing up OneDrive to \"{0}\"", _rootPath);

                downloadTasks = new List<Task>();

                var root = _graphClient.Me.Drive.Root.Request().GetAsync().Result;

                if (root.Size.HasValue && _reportingEnabled)
                {
                    _lastReported = _downloaded = 0;
                    _total = root.Size.Value;
                }
                else
                {
                    if (_reportingEnabled)
                        _logger.LogWarning("Could not get size of root folder. Progress is not printed.");
                    _total = 0;
                }

                await DownloadAllRecursive(root, "");

                _logger.LogDebug("Awaiting downloads to finish...");
                Task.WaitAll(downloadTasks.ToArray(), _ct);

                _logger.LogInformation("Backup finished.");
            }
            catch (ServiceException ex)
            {
                _logger.LogCritical("Could not get root item of OneDrive. Nothing could be downloaded.\n{0}", ex);
            }
        }

        private void ReportProgress(long finishedDownloading)
        {
            if (_total != 0)
            {
                _downloaded += finishedDownloading;
                var div = decimal.Divide(_downloaded, _total);
                if (div.CompareTo(_lastReported + _reportingSteps) != -1)
                {
                    _lastReported = _reportingSteps * Math.Floor(div / _reportingSteps);
                    _logger.LogInformation($"{_lastReported * 100}% - {_downloaded / 1000000}/{_total / 1000000}Mb");
                }
            }
        }

    }
}