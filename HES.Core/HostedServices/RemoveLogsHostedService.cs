using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class RemoveLogsHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<RemoveLogsHostedService> _logger;
        private readonly string _path;
        private Timer _timer;

        public RemoveLogsHostedService(ILogger<RemoveLogsHostedService> logger)
        {
            _logger = logger;
            _path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "logs");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                if (Directory.Exists(_path))
                {
                    var files = new DirectoryInfo(_path).GetFiles("*.log").Where(x => x.CreationTime < DateTime.Now.AddDays(-30)).ToList();

                    foreach (var file in files)
                    {
                        File.Delete(file.FullName);
                        _logger.LogInformation($"File has been deleted {file.Name}");
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
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}