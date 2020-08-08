using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Ran at: {time}", DateTimeOffset.Now);

                var authProvider = new DeviceCodeAuthProvider(AppData.MsGraphConfig.ClientId, AppData.MsGraphConfig.Scopes);
                await authProvider.InitializeAuthentication();


                DeleteOldestFolders(AppData.BackupConfig.Path, AppData.BackupConfig.RemainMaximum);
                var graphHelper = new GraphHelper<BackupService>(_logger, authProvider);
                await graphHelper.DownloadAll($@"{AppData.BackupConfig.Path}/{GetBackupDirectoryName()}");
            }
            finally
            {
                AppData.Lifetime.StopApplication();
            }

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}


        }

        private void DeleteOldestFolders(string path, int max)
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
            while (dirCount - removed >= max && dirDict.Count > 0)
            {
                var oldest = dirDict.First();
                try
                {
                    dirDict.Remove(oldest.Key);
                    Directory.Delete($@"{path}/{oldest.Key}", true);
                    _logger.LogInformation("Removed backup \"{0}\".", oldest.Key);
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
            folderName = folderName.Remove(0, Constants.BackupFolderPrefix.Length).Replace('_', ' ');
            var split = folderName.Split(' ');
            var time = split[1].Replace('-', ':');
            return $"{split[0]} {time}";
        }

        private string GetBackupDirectoryName()
        {
            var s = DateTime.Now.ToString("u", CultureInfo.CreateSpecificCulture("en-US")).Replace(':', '-').Replace(' ', '_');
            return Constants.BackupFolderPrefix + s.Remove(s.Length - 1);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken);
        }
    }
}
