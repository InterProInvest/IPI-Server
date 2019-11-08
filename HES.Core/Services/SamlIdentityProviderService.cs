using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SamlIdentityProviderService : ISamlIdentityProviderService
    {
        private readonly IAsyncRepository<SamlIdentityProvider> _samlIdentityProviderRepository;

        public SamlIdentityProviderService(IAsyncRepository<SamlIdentityProvider> samlIdentityProviderRepository)
        {
            _samlIdentityProviderRepository = samlIdentityProviderRepository;
        }

        public IQueryable<SamlIdentityProvider> Query()
        {
            return _samlIdentityProviderRepository.Query();
        }

        public async Task<SamlIdentityProvider> GetByIdAsync(dynamic id)
        {
            return await _samlIdentityProviderRepository.GetByIdAsync(id);
        }

        public async Task<bool> GetStatusAsync()
        {
            var idp = await _samlIdentityProviderRepository.GetByIdAsync(SamlIdentityProvider.PrimaryKey);
            return idp.Enabled;
        }

        public async Task UpdateSamlIdentityProviderAsync(SamlIdentityProvider identityProvider)
        {
            if (identityProvider == null)
            {
                throw new ArgumentNullException(nameof(identityProvider));
            }

            await _samlIdentityProviderRepository.UpdateAsync(identityProvider);
        }
    }
}