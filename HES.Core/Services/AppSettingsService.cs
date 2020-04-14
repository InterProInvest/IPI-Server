using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IAsyncRepository<AppSettings> _appSettingsRepository;

        public AppSettingsService(IAsyncRepository<AppSettings> appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<LicensingSettings> GetLicensingSettingsAsync()
        {
            var licensing = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Licensing);
            if (licensing == null)
            {
                return new LicensingSettings();
            }
            return JsonConvert.DeserializeObject<LicensingSettings>(licensing.Value);
        }

        public async Task SetLicensingSettingsAsync(LicensingSettings licensing)
        {
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
            {
                return new ServerSettings();
            }
            return JsonConvert.DeserializeObject<ServerSettings>(server.Value);
        }

        public async Task SetServerSettingsAsync(ServerSettings server)
        {
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

        public async Task<DomainSettings> GetDomainSettingsAsync()
        {
            var domain = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Domain);
            if (domain == null)
            {
                return new DomainSettings();
            }
            return JsonConvert.DeserializeObject<DomainSettings>(domain.Value);
        }

        public async Task SetDomainSettingsAsync(DomainSettings domain)
        {
            var json = JsonConvert.SerializeObject(domain);

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