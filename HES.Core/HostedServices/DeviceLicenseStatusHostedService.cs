using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class DeviceLicenseStatusHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly ILicenseService _licenseService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<LicenseOrderStatusHostedService> _logger;
        private Timer _timer;

        public DeviceLicenseStatusHostedService(IServiceProvider services, ILogger<LicenseOrderStatusHostedService> logger)
        {
            _scope = services.CreateScope();
            _licenseService = _scope.ServiceProvider.GetRequiredService<ILicenseService>();
            _emailSenderService = _scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
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
                var devices = await _licenseService.UpdateDeviceLicenseStatusAsync();
                await _emailSenderService.SendDeviceLicenseStatus(devices);
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
            _scope.Dispose();
        }
    }
}
