using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplatesPage : OwningComponentBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        public IMainTableService<Template, TemplateFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
            MainTableService = ScopedServices.GetRequiredService<IMainTableService<Template, TemplateFilter>>();

            await InitializeHubAsync();
            await BreadcrumbsService.SetTemplates();
            await MainTableService.InitializeAsync(TemplateService.GetTemplatesAsync, TemplateService.GetTemplatesCountAsync, ModalDialogService, StateHasChanged, nameof(Template.Name), ListSortDirection.Ascending);
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
                builder.AddAttribute(1, nameof(EditTemplate.TemplateId), MainTableService.SelectedEntity.Id);
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
                builder.AddAttribute(1, nameof(DeleteTemplate.TemplateId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteTemplate.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Template", body, ModalDialogSize.Default);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Templates, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}