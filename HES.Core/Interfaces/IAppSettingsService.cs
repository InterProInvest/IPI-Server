using HES.Core.Models.Web.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<Licensing> GetLicensingSettingsAsync();
        Task SetLicensingSettingsAsync(Licensing licensing);
        Task<Server> GetServerSettingsAsync();
        Task SetServerSettingsAsync(Server server);
    }
}