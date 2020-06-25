using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplatesPage : ComponentBase
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMainTableService<Template, TemplateFilter> MainTableService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(TemplateService.GetTemplatesAsync, TemplateService.GetTemplatesCountAsync, StateHasChanged, nameof(Template.Name), ListSortDirection.Ascending);
            await BreadcrumbsService.SetTemplates();
            await InitializeHubAsync();
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.Templates, async (connectionId) =>
            {
                var id = hubConnection.ConnectionId;
                if (id != connectionId)
                {
                    await TemplateService.DetachTemplatesAsync(MainTableService.Entities);
                    await MainTableService.LoadTableDataAsync();
                    StateHasChanged();
                    ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
                }
            });

            await hubConnection.StartAsync();
        }

        private async Task CreateTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateTemplate));
                builder.AddAttribute(1, nameof(CreateTemplate.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Template", body, ModalDialogSize.Default);
        }

        private async Task EditTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditTemplate));
                builder.AddAttribute(1, nameof(EditTemplate.Template), MainTableService.SelectedEntity);
                builder.AddAttribute(2, nameof(EditTemplate.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Template", body, ModalDialogSize.Default);
        }

        private async Task DeleteTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteTemplate));
                builder.AddAttribute(1, nameof(DeleteTemplate.Template), MainTableService.SelectedEntity);
                builder.AddAttribute(2, nameof(DeleteTemplate.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Template", body, ModalDialogSize.Default);
        }
    }
}
