using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class IndexModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        public IList<LicenseOrder> LicenseOrder { get; set; }

        public IndexModel(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        public async Task OnGetAsync()
        {
            LicenseOrder = await _licenseService.GetLicenseOrdersAsync();
        }

        public async Task<IActionResult> OnPostSendOrderAsync(string orderId)
        {
            var order = await _licenseService.GetLicenseOrderByIdAsync(orderId);
            var devices = await _licenseService.GetDeviceLicensesByOrderIdAsync(orderId);

            var licenseOrderDto = new LicenseOrderDto()
            {
                Id = order.Id,
                ContactEmail = order.ContactEmail,
                CustomerNote = order.Note,
                LicenseStartDate = order.StartDate,
                LicenseEndDate = order.EndDate,
                ProlongExistingLicenses = order.ProlongExistingLicenses,
                CustomerId = "BBB26599-81B8-44D5-80C0-31CF830F1578",
                Devices = devices.Select(d => d.DeviceId).ToList()
            };

            string apiUrl = "https://localhost:44388/api/Licenses/CreateLicenseOrder";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var stringContent = new StringContent(JsonConvert.SerializeObject(licenseOrderDto), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, stringContent);
                //if (response.IsSuccessStatusCode)
                //{
                //    var data = await response.Content.ReadAsStringAsync();
                //    var table = JsonConvert.DeserializeObject<System.Data.DataTable>(data);
                //}
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await _licenseService.SetStatusSent(order);
                }
            }

            return RedirectToPage("./Index");
        }
    }
}