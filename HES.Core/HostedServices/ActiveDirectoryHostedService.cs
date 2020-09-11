using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class ActiveDirectoryHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActiveDirectoryHostedService> _logger;
        private Timer _timer;

        public ActiveDirectoryHostedService(IServiceProvider services, ILogger<ActiveDirectoryHostedService> logger)
        {
            _serviceProvider = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(120));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
                var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();


                var ldapSettings = await appSettingsService.GetLdapSettingsAsync();

                if (ldapSettings == null)
                    throw new Exception("AD settings is null");

                await ldapService.ChangePasswordWhenExpiredAsync("dfa5d5b8-ab5f-47fd-98b2-7ffd34b8930e", ldapSettings);
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
