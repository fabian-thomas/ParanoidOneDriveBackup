using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MAB.DotIgnore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParanoidOneDriveBackup.App;

namespace ParanoidOneDriveBackup
{
    public class BackupService : BackgroundService
    {
        private ILogger<BackupService> _logger;

        public BackupService(IHostApplicationLifetime hostApplicationLifetime, ILogger<BackupService> logger, IConfiguration config)
        {
            _logger = logger;
            AppData.Lifetime = hostApplicationLifetime;

            AppData.BindConfig(config);
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogDebug("authing");

                // Authentication
                var tokenCacheHelper = new TokenCacheHelper<BackupService>(_logger, Constants.TOKEN_CACHE_FILE_PATH);
                var authProvider = new DeviceCodeAuthProvider<BackupService>(AppData.MsGraphConfig.ClientId, AppData.MsGraphConfig.Scopes, _logger, tokenCacheHelper);
                var authenticated = await authProvider.InitializeAuthentication();

                // authenticate exportimport for notebook downloads
                string exportImportToken = null;
                if (AppData.BackupConfig.NotebookDownloadEnabled)
                {
                    if (!File.Exists(Constants.EXPORT_IMPORT_TOKEN_CACHE_FILE_PATH) || true)
                    {
                        Console.WriteLine($"Log in to your microsoft account and navigate to https://login.live.com/oauth20_authorize.srf?client_id={AppData.ExportImportConfig.ClientId}&scope={string.Join(",", AppData.ExportImportConfig.Scopes)}&response_type=token in a browser. Paste the redirected url in the console.");

                        // response should look like this: https://www.onenote.com/notebooks/exportimport#access_token=<token>&token_type=bearer&expires_in=3600&scope=onedrive_implicit.access&user_id=<user_id>
                        exportImportToken = HttpUtility.ParseQueryString(new Uri(Console.ReadLine()).Fragment.Remove(0, 1)).Get("access_token"); // remove proceeding # from fragment

                        File.WriteAllText(Constants.EXPORT_IMPORT_TOKEN_CACHE_FILE_PATH, exportImportToken);
                    }
                    else
                    {
                        exportImportToken = File.ReadAllText(Constants.EXPORT_IMPORT_TOKEN_CACHE_FILE_PATH);
                    }

                    if (exportImportToken == null)
                        throw new NotImplementedException(); //TODO
                }

                _logger.LogDebug("after authing");

                // Backup
                if (!ct.IsCancellationRequested && authenticated)
                {
                    try
                    {
                        Directory.CreateDirectory(AppData.BackupConfig.Path);
                        DeleteOldestFolders(AppData.BackupConfig.Path, AppData.BackupConfig.RemainMaximum, ct);
                        if (!ct.IsCancellationRequested)
                        {
                            // directy info class is used to normalize slashes in path
                            var graphHelper = new GraphHelper<BackupService>(_logger, authProvider, ct, new DirectoryInfo(Path.Combine(AppData.BackupConfig.Path, GetBackupDirectoryName())).FullName, AppData.BackupConfig.MaxParallelDownloadTasks,
                                AppData.BackupConfig.ProgressReporting.ProgressReportingSteps, AppData.BackupConfig.ProgressReporting.Enabled, exportImportToken);
                            await graphHelper.DownloadAll();
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogCritical("Could not access or create backup folder \"{0}\"\n", AppData.BackupConfig.Path, ex);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Unhandled exception in backup service:\n{0}", ex);
            }

            _logger.LogDebug("shutting down");
            AppData.Lifetime.StopApplication();
        }

        private void DeleteOldestFolders(string path, int max, CancellationToken ct)
        {
            var dirs = Directory.GetDirectories(path).Select(dir => Path.GetFileName(dir));
            SortedDictionary<string, DateTime> dirDict = new SortedDictionary<string, DateTime>();
            foreach (var dir in dirs)
            {
                try
                {
                    DateTime d;
                    if (DateTime.TryParse(ParseBackupDirectoryName(dir), out d))
                        dirDict.Add(dir, d);
                }
                catch (Exception)
                {
                    _logger.LogWarning("Could not parse suffix of directory {0} to date. Ignoring this directory.", dir);
                }
            }

            int removed = 0;
            int dirCount = dirDict.Count;
            // remove as much as needed, ignore dirs that throw exception
            while (dirCount - removed >= max && dirDict.Count > 0 && !ct.IsCancellationRequested)
            {
                var oldest = dirDict.First();
                try
                {
                    dirDict.Remove(oldest.Key);
                    Directory.Delete(Path.Combine(path, oldest.Key), true);
                    _logger.LogInformation("Removed backup \"{0}\"", oldest.Key);
                    removed++;
                }
                catch (IOException ex)
                {
                    _logger.LogError("Could not delete directory \"{0}\".\n{1}", oldest.Key, ex);
                }
            }
        }

        private string ParseBackupDirectoryName(string folderName)
        {
            folderName = folderName.Remove(0, Constants.BACKUP_DIR_PREFIX.Length).Replace('_', ' ');
            var split = folderName.Split(' ');
            var time = split[1].Replace('-', ':');
            return $"{split[0]} {time}";
        }

        private string GetBackupDirectoryName()
        {
            var s = DateTime.Now.ToString("u", CultureInfo.CreateSpecificCulture("en-US")).Replace(':', '-').Replace(' ', '_');
            return Constants.BACKUP_DIR_PREFIX + s.Remove(s.Length - 1);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // read ignore file
            if (File.Exists(Constants.IGNORE_FILE_PATH))
                AppData.Ignore = new IgnoreList(Constants.IGNORE_FILE_PATH);
            else
                AppData.Ignore = new IgnoreList();

            await ExecuteAsync(cancellationToken);
        }
    }
}
