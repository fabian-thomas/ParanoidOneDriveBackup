using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ParanoidOneDriveBackup
{
    public class Worker : BackgroundService
    {

        public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger, IConfiguration config)
        {
            AppData.Logger = logger;
            AppData.Lifetime = hostApplicationLifetime;

            AppData.BindConfig(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                AppData.Logger.LogInformation("Ran at: {time}", DateTimeOffset.Now);

                var authProvider = new DeviceCodeAuthProvider(AppData.MsGraphConfig.ClientId, AppData.MsGraphConfig.Scopes);
                await authProvider.InitializeAuthentication();

                GraphHelper.Initialize(authProvider);

                var path = string.Format(@"{0}\download", Environment.CurrentDirectory);
                await GraphHelper.DownloadAll(path);
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

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken);
        }
    }
}
