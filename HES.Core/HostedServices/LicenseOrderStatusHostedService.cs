using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class LicenseOrderStatusHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IServiceScope _scope;
        private readonly ILicenseService _licenseService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<LicenseOrderStatusHostedService> _logger;
        private Timer _timer;

        public LicenseOrderStatusHostedService(ILogger<LicenseOrderStatusHostedService> logger, IServiceProvider services)
        {
            _services = services;
            _logger = logger;

            _scope = _services.CreateScope();
            _licenseService = _scope.ServiceProvider.GetRequiredService<ILicenseService>();
            _emailSenderService = _scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                var orders = await _licenseService.GetOpenLicenseOrdersAsync();

                foreach (var order in orders)
                {
                    var status = await _licenseService.GetLicenseOrderStatusAsync(order.Id);

                    // Http transport error
                    if (status == OrderStatus.Undefined)
                    {
                        continue;
                    }

                    // Status has not changed
                    if (status == order.OrderStatus)
                    {
                        continue;
                    }

                    if (status == OrderStatus.Completed)
                    {
                        await _licenseService.UpdateNewDeviceLicensesAsync(order.Id);
                    }

                    await _licenseService.ChangeOrderStatusAsync(order, status);    
                    await _emailSenderService.SendLicenseChangedAsync(order.CreatedAt, status);
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
            _scope.Dispose();
        }
    }
}
