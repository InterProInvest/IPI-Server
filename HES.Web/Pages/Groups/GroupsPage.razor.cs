using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Group;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : OwningComponentBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        public IMainTableService<Group, GroupFilter> MainTableService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GroupsPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Group, GroupFilter>>();

                await InitializeHubAsync();
                await BreadcrumbsService.SetGroups();
                await MainTableService.InitializeAsync(GroupService.GetGroupsAsync, GroupService.GetGroupsCountAsync, ModalDialogService, StateHasChanged, nameof(Group.Name));

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private Task NavigateToGroupDetails()
        {
            NavigationManager.NavigateTo($"/Groups/Details/{MainTableService.SelectedEntity.Id}");
            return Task.CompletedTask;
        }

        private async Task OpenModalAddGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add group", body);
        }

        private async Task OpenModalCreateGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
        }

        private async Task OpenModalEditGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, nameof(DeleteGroup.GroupId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit group", body);
        }

        private async Task OpenModalDeleteGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, nameof(DeleteGroup.GroupId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete group", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Groups, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
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