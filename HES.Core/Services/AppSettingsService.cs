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

        public async Task<Licensing> GetLicensingSettingsAsync()
        {
            var licensing = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Licensing);
            if (licensing == null)
            {
                return new Licensing();
            }
            return JsonConvert.DeserializeObject<Licensing>(licensing.Value);
        }

        public async Task SetLicensingSettingsAsync(Licensing licensing)
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

        public async Task<Server> GetServerSettingsAsync()
        {
            var server = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Server);
            if (server == null)
            {
                return new Server();
            }
            return JsonConvert.DeserializeObject<Server>(server.Value);
        }

        public async Task SetServerSettingsAsync(Server server)
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

        public async Task<Domain> GetDomainSettingsAsync()
        {
            var domain = await _appSettingsRepository.GetByIdAsync(AppSettingsConstants.Domain);
            if (domain == null)
            {
                return new Domain();
            }
            return JsonConvert.DeserializeObject<Domain>(domain.Value);
        }

        public async Task SetDomainSettingsAsync(Domain domain)
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