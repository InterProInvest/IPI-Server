using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISamlIdentityProviderService
    {
        IQueryable<SamlIdentityProvider> Query();
        Task<SamlIdentityProvider> GetByIdAsync(dynamic id);
        Task<bool> GetStatusAsync();
        Task UpdateSamlIdentityProviderAsync(SamlIdentityProvider identityProvider);
    }
}