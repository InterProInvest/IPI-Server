using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class ActiveDirectoryHostedService : IHostedService, IDisposable
    {
        private readonly int _intervalInHours = 12;
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
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(_intervalInHours));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var groupService = scope.ServiceProvider.GetRequiredService<IGroupService>();

                    var status = await groupService.GetAutoPasswordChangeStatusAsync();
                    if (!status)
                        return;

                    var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();

                    var ldapSettings = await appSettingsService.GetLdapSettingsAsync();
                    if (ldapSettings == null)
                    {
                        _logger.LogWarning("Active Directory credentials no set");
                        return;
                    }

                    var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
                    await ldapService.ChangePasswordWhenExpiredAsync(ldapSettings);
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