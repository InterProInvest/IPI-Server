using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IAsyncRepository<AppSettings> _appSettingsRepository;
        private readonly IDataProtectionService _dataProtectionService;

        public AppSettingsService(IAsyncRepository<AppSettings> appSettingsRepository, IDataProtectionService dataProtectionService)
        {
            _appSettingsRepository = appSettingsRepository;
            _dataProtectionService = dataProtectionService;
        }

        public async Task<LicensingSettings> GetLicensingSettingsAsync()
        {
            var licensing = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Licensing);

            if (licensing == null)
                return null;

            return JsonConvert.DeserializeObject<LicensingSettings>(licensing.Value);
        }

        public async Task SetLicensingSettingsAsync(LicensingSettings licensing)
        {
            if (licensing == null)
                throw new ArgumentNullException(nameof(LicensingSettings));

            var json = JsonConvert.SerializeObject(licensing);

            var appSettings = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Licensing);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = AppSettingsConstants.Licensing,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public async Task<ServerSettings> GetServerSettingsAsync()
        {
            var server = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Server);

            if (server == null)
                return null;

            return JsonConvert.DeserializeObject<ServerSettings>(server.Value);
        }

        public async Task SetServerSettingsAsync(ServerSettings server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(ServerSettings));

            var json = JsonConvert.SerializeObject(server);

            var appSettings = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Server);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = AppSettingsConstants.Server,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public async Task<LdapSettings> GetDomainSettingsAsync()
        {
            var domain = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Domain);

            if (domain == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<LdapSettings>(domain.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetDomainSettingsAsync(LdapSettings ldapSettings)
        {
            if (ldapSettings == null)
                throw new ArgumentNullException(nameof(LdapSettings));

            ldapSettings.Password = _dataProtectionService.Encrypt(ldapSettings.Password);

            var json = JsonConvert.SerializeObject(ldapSettings);

            var appSettings = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Domain);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = AppSettingsConstants.Domain,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }
    }
}