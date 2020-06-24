using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class DetailsModel : PageModel
    {
        private readonly IWorkstationService _workstationService;

        public string WorkstationId { get; set; }

        public IList<WorkstationProximityVault> ProximityDevices { get; set; }
        public IList<HardwareVault> Devices { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationProximityVault ProximityDevice { get; set; }
        public bool WarningMessage { get; set; }


        public DetailsModel(IWorkstationService workstationService)
        {
            _workstationService = workstationService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                return NotFound();
            }

            WorkstationId = id;

            return Page();
        }
      
        //public async Task<IActionResult> OnGetDeleteProximityDeviceAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        _logger.LogWarning($"{nameof(id)} is null");
        //        return NotFound();
        //    }

        //    ProximityDevice = await _workstationService
        //        .ProximityVaultQuery()
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (ProximityDevice == null)
        //    {
        //        _logger.LogWarning($"{nameof(ProximityDevice)} is null");
        //        return NotFound();
        //    }

        //    return Partial("_DeleteProximityDevice", this);
        //}

        //public async Task<IActionResult> OnPostDeleteProximityDeviceAsync(WorkstationProximityVault proximityDevice)
        //{
        //    if (proximityDevice == null)
        //    {
        //        _logger.LogWarning($"{nameof(proximityDevice)} is null");
        //        return NotFound();
        //    }

        //    try
        //    {
        //        await _workstationService.DeleteProximityVaultAsync(proximityDevice.Id);
        //        SuccessMessage = $"Proximity device removed.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    var id = proximityDevice.WorkstationId;
        //    return RedirectToPage("./Details", new { id });
        //}
    }
}