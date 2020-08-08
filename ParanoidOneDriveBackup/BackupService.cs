using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            // Authentication
            var authProvider = new DeviceCodeAuthProvider(AppData.MsGraphConfig.ClientId, AppData.MsGraphConfig.Scopes);
            await authProvider.InitializeAuthentication();

            // Backup
            if (!ct.IsCancellationRequested)
            {
                DeleteOldestFolders(AppData.BackupConfig.Path, AppData.BackupConfig.RemainMaximum, ct);
                if (!ct.IsCancellationRequested)
                {
                    var graphHelper = new GraphHelper<BackupService>(_logger, authProvider, ct, $@"{AppData.BackupConfig.Path}/{GetBackupDirectoryName()}");
                    await graphHelper.DownloadAll();
                }
            }

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
                    Directory.Delete($@"{path}/{oldest.Key}", true);
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
            ExecuteAsync(cancellationToken);
        }
    }
}
