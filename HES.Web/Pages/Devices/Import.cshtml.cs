using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class ImportModel : PageModel
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<ImportModel> _logger;


        public IList<Device> DevicesExists { get; set; }
        public IList<Device> DevicesImported { get; set; }
        public string Message { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Choose file")]
            public IFormFile FileToUpload { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public ImportModel(IDeviceService deviceService, ILogger<ImportModel> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var key = Input.Password;
                var file = Input.FileToUpload;
                var contentType = file.ContentType;

                if (contentType == "application/octet-stream")
                {
                    using (var fileStream = file.OpenReadStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);

                            try
                            {
                                byte[] fileContent = memoryStream.ToArray();
                                // Device import
                                var result = await _deviceService.ImportDevicesAsync(key, fileContent);
                                // Set result
                                DevicesExists = result.devicesExists;
                                DevicesImported = result.devicesImported;
                                Message = result.message;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.Message);
                                Message = $"There is a problem with device import.{Environment.NewLine}" +
                                          $"Please, check if you select a correct file, enter correct encryption key and try again.";
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Incorrect format file");
                    Message = "Selected file is not in correct format. Please, select .hdz file and try again.";
                }
            }
            return Page();
        }
    }
}