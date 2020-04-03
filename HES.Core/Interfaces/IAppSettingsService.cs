using HES.Core.Models.Web.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<Licensing> GetLicensingSettingsAsync();
        Task SetLicensingSettingsAsync(Licensing licensing);
        Task<ServerSettings> GetServerSettingsAsync();
        Task SetServerSettingsAsync(ServerSettings server);
        Task<Domain> GetDomainSettingsAsync();
        Task SetDomainSettingsAsync(Domain domain);
    }
}