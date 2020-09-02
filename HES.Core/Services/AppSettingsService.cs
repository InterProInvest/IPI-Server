using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.EntityFrameworkCore;
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
            var licensing = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Licensing);

            if (licensing == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<LicensingSettings>(licensing.Value);
            deserialized.ApiKey = _dataProtectionService.Decrypt(deserialized.ApiKey);

            return deserialized;
        }

        public async Task SetLicensingSettingsAsync(LicensingSettings licensing)
        {
            if (licensing == null)
                throw new ArgumentNullException(nameof(LicensingSettings));

            licensing.ApiKey = _dataProtectionService.Encrypt(licensing.ApiKey);

            var json = JsonConvert.SerializeObject(licensing);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Licensing);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Licensing,
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

        public async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Email);

            if (settings == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<EmailSettings>(settings.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetEmailSettingsAsync(EmailSettings email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            email.Password = _dataProtectionService.Encrypt(email.Password);

            var json = JsonConvert.SerializeObject(email);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Email);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Email,
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
            var server = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Server);

            if (server == null)
                return null;

            return JsonConvert.DeserializeObject<ServerSettings>(server.Value);
        }

        public async Task SetServerSettingsAsync(ServerSettings server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(ServerSettings));

            var json = JsonConvert.SerializeObject(server);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Server);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Server,
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

        public async Task<LdapSettings> GetLdapSettingsAsync()
        {
            var domain = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Domain);

            if (domain == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<LdapSettings>(domain.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetLdapSettingsAsync(LdapSettings ldapSettings)
        {
            if (ldapSettings == null)
                throw new ArgumentNullException(nameof(LdapSettings));

            ldapSettings.Password = _dataProtectionService.Encrypt(ldapSettings.Password);

            var json = JsonConvert.SerializeObject(ldapSettings);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Domain);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Domain,
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