using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class DeletePersonalDataPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<DeletePersonalDataPage> Logger { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public RequiredPassword Input { get; set; } = new RequiredPassword();
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

        private async Task DeletePersonalDataAsync()
        {
            try
            {
                List<string> cookies = null;
                if (HttpClient.DefaultRequestHeaders.TryGetValues("Cookie", out IEnumerable<string> cookieEntries))
                    cookies = cookieEntries.ToList();

                var response = await HttpClient.PostAsync("api/Identity/DeletePersonalData", (new StringContent(JsonConvert.SerializeObject(Input), Encoding.UTF8, "application/json")));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                if (cookies != null && cookies.Any())
                {
                    HttpClient.DefaultRequestHeaders.Remove("Cookie");

                    foreach (var cookie in cookies[0].Split(';'))
                    {
                        var cookieParts = cookie.Split('=');
                        await JSRuntime.InvokeVoidAsync("removeCookie", cookieParts[0]);
                    }
                }

                NavigationManager.NavigateTo("Identity/Account/Login", true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}