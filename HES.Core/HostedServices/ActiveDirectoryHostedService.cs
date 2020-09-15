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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActiveDirectoryHostedService> _logger;

        private const int _changePasswordIntervalInHours = 12;
        private const int _synchronizationUsersIntervalInHours = 1;

        private Timer _changePassworTimer;
        private Timer _synchronizationUsersTimer;

        public ActiveDirectoryHostedService(IServiceProvider services, ILogger<ActiveDirectoryHostedService> logger)
        {
            _logger = logger;
            _serviceProvider = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _changePassworTimer = new Timer(async (object state) => await ChangePasswordAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(_changePasswordIntervalInHours));
            _synchronizationUsersTimer = new Timer(async (object state) => await SynchronizationUsersAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(_synchronizationUsersIntervalInHours));
            return Task.CompletedTask;
        }

        private async Task ChangePasswordAsync()
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

        private async Task SynchronizationUsersAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();

                    var ldapSettings = await appSettingsService.GetLdapSettingsAsync();
                    if (ldapSettings == null)
                    {
                        _logger.LogWarning("Active Directory credentials no set");
                        return;
                    }

                    var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
                    await ldapService.SynchronizationUsersAsync(ldapSettings);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changePassworTimer?.Change(Timeout.Infinite, 0);
            _synchronizationUsersTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _changePassworTimer?.Dispose();
            _synchronizationUsersTimer?.Dispose();
        }
    }
}