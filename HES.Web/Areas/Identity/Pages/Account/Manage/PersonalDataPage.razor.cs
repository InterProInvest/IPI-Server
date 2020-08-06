using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class PersonalDataPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<PersonalDataPage> Logger { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync("api/Identity/GetUser");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                ApplicationUser = JsonConvert.DeserializeObject<ApplicationUser>(await response.Content.ReadAsStringAsync());
                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                LoadFailed = true;
            }
        }

        private async Task DownloadPersonalDataASync()
        {
            try
            {
                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(ApplicationUser)?.ToString() ?? "null");
                }

                await JSRuntime.InvokeVoidAsync("downloadPersonalData", JsonConvert.SerializeObject(personalData));

                ToastService.ShowToast("Download started.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}