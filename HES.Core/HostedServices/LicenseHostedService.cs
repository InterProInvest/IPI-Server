using HES.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class LicenseHostedService : IHostedService, IDisposable
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<LicenseHostedService> _logger;
        private Timer _timer;

        public LicenseHostedService(ILicenseService licenseService,
                                    ILogger<LicenseHostedService> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                await _licenseService.UpdateLicenseOrdersAsync();
                await _licenseService.UpdateDeviceLicenseStatusAsync();
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
