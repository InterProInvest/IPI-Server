using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.IdentityProvider
{
    public class IndexModel : PageModel
    {
        private readonly ISamlIdentityProviderService _samlIdentityProviderService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IEmployeeService _employeeService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;

        public SamlIdentityProvider SamlIdentityProvider { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISamlIdentityProviderService samlIdentityProviderService,
                          ILogger<IndexModel> logger,
                          IEmployeeService employeeService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService)
        {
            _samlIdentityProviderService = samlIdentityProviderService;
            _logger = logger;
            _employeeService = employeeService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
        }

        public async Task OnGet()
        {
            SamlIdentityProvider = await _samlIdentityProviderService
                .Query()
                .FirstOrDefaultAsync();
        }

        public async Task<IActionResult> OnPostEditSamlIdentityProviderAsync(SamlIdentityProvider samlIdentityProvider)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }
            try
            {
                ValidationHepler.VerifyUrls(samlIdentityProvider.Url);

                var currentIdentityProvider = await _samlIdentityProviderService
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == SamlIdentityProvider.PrimaryKey);

                await _samlIdentityProviderService.UpdateSamlIdentityProviderAsync(samlIdentityProvider);

                if (currentIdentityProvider.Url != samlIdentityProvider.Url)
                {
                    var devices = await _employeeService.UpdateUrlSamlIdpAccountAsync(Request.Host.Value);
                    _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                }

                SuccessMessage = $"SAML IdP settings updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }
            return RedirectToPage("./Index");
        }
    }
}