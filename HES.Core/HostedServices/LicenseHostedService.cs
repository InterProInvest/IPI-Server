﻿using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class LicenseHostedService : IHostedService, IDisposable
    {
        public IServiceProvider Services { get; }

        private readonly ILogger<LicenseHostedService> _logger;
        private Timer _timer;

        public LicenseHostedService(IServiceProvider services, ILogger<LicenseHostedService> logger)
        {
            Services = services;
            _logger = logger;
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
                using var scope = Services.CreateScope();
                var scopedLicenseService = scope.ServiceProvider.GetRequiredService<ILicenseService>();

                await scopedLicenseService.UpdateLicenseOrdersAsync();
                await scopedLicenseService.UpdateHardwareVaultsLicenseStatusAsync();
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