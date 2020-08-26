using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MAB.DotIgnore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParanoidOneDriveBackup.App;
using ParanoidOneDriveBackup.Authentication;
using ParanoidOneDriveBackup.Graph;

namespace ParanoidOneDriveBackup
{
    public class BackupService : BackgroundService
    {
        private readonly ILogger<BackupService> _logger;

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
                var tokenCacheHelper = new TokenCacheHelper<BackupService>(_logger, Constants.TokenCacheFilePath);
                var authProvider = new DeviceCodeAuthProvider<BackupService>(AppData.MsGraphConfig.ClientId, AppData.MsGraphConfig.Scopes, _logger, tokenCacheHelper);
                var authenticated = await authProvider.InitializeAuthentication();

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
                            // directly info class is used to normalize slashes in path
                            var graphHelper = new GraphHelper<BackupService>(_logger, authProvider, ct, new DirectoryInfo(Path.Combine(AppData.BackupConfig.Path, GetBackupDirectoryName())).FullName, AppData.BackupConfig.MaxParallelDownloadTasks,
                                AppData.BackupConfig.ProgressReporting.ProgressReportingSteps, AppData.BackupConfig.ProgressReporting.Enabled);
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
            var dirs = Directory.GetDirectories(path).Select(Path.GetFileName);
            var dirDict = new SortedDictionary<string, DateTime>();
            foreach (var dir in dirs)
            {
                try
                {
                    if (DateTime.TryParse(ParseBackupDirectoryName(dir), out var d))
                        dirDict.Add(dir, d);
                }
                catch (Exception)
                {
                    _logger.LogWarning("Could not parse suffix of directory {0} to date. Ignoring this directory.", dir);
                }
            }

            var removed = 0;
            var dirCount = dirDict.Count;
            // remove as much as needed, ignore dirs that throw exception
            while (dirCount - removed >= max && dirDict.Count > 0 && !ct.IsCancellationRequested)
            {
                var (key, _) = dirDict.First();
                try
                {
                    dirDict.Remove(key);
                    Directory.Delete(Path.Combine(path, key), true);
                    _logger.LogInformation("Removed backup \"{0}\"", key);
                    removed++;
                }
                catch (IOException ex)
                {
                    _logger.LogError("Could not delete directory \"{0}\".\n{1}", key, ex);
                }
            }
        }

        private string ParseBackupDirectoryName(string folderName)
        {
            folderName = folderName.Remove(0, Constants.BackupDirPrefix.Length).Replace('_', ' ');
            var split = folderName.Split(' ');
            var time = split[1].Replace('-', ':');
            return $"{split[0]} {time}";
        }

        private string GetBackupDirectoryName()
        {
            var s = DateTime.Now.ToString("u", CultureInfo.CreateSpecificCulture("en-US")).Replace(':', '-').Replace(' ', '_');
            return Constants.BackupDirPrefix + s.Remove(s.Length - 1);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // read ignore file
            AppData.Ignore = File.Exists(Constants.IgnoreFilePath) ? new IgnoreList(Constants.IgnoreFilePath) : new IgnoreList();

            await ExecuteAsync(cancellationToken);
        }
    }
}
