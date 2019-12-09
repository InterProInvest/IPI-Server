using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoveLogsFilesHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _folderPath;
        private Timer _timer;

        public RemoveLogsFilesHostedService(ILogger<RemoveLogsFilesHostedService> logger)
        {
            _logger = logger;
            _folderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "logs");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Remove logs service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(_folderPath);
                FileInfo[] fileInfo = directoryInfo.GetFiles("*.log");

                foreach (var item in fileInfo)
                {
                    if (item.CreationTime < DateTime.Now.AddDays(-30))
                    {
                        File.Delete(item.FullName);
                        _logger.LogInformation($"File deleted {item.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Remove logs service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}