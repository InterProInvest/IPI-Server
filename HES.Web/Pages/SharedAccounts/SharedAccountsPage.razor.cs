using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Pages.Employees;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class SharedAccountsPage : ComponentBase
    {
        [Inject] public IMainTableService<SharedAccount, SharedAccountsFilter> MainTableService { get; set; }
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public ILogger<SharedAccountsPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(SharedAccountService.GetSharedAccountsAsync, SharedAccountService.GetSharedAccountsCountAsync, StateHasChanged, nameof(SharedAccount.Name), ListSortDirection.Ascending);
            await BreadcrumbsService.SetSharedAccounts();
        }


        private async Task CreateSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateSharedAccount));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Shared Account", body, ModalDialogSize.Large);
        }
    }
}
