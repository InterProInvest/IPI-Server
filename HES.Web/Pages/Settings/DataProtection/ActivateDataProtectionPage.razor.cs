﻿using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class ActivateDataProtectionPage : ComponentBase
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ActivateDataProtectionPage> Logger { get; set; }

        public InputModel Input { get; set; } = new InputModel();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            var status = DataProtectionService.Status();

            if (status != ProtectionStatus.Activate)
                NavigationManager.NavigateTo("");
        }

        private async Task ActivateAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var result = await DataProtectionService.ActivateProtectionAsync(Input.Password);
                    if (!result)
                    {
                        ValidationErrorMessage.DisplayError(nameof(InputModel.Password), "Invalid password");
                        return;
                    }
                    NavigationManager.NavigateTo("");
                });

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
