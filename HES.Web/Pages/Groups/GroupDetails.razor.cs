using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Groups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupDetails : OwningComponentBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        public IMainTableService<GroupMembership, GroupMembershipFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GroupDetails> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Group Group { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<GroupMembership, GroupMembershipFilter>>();

                await InitializeHubAsync();
                await LoadGroupAsync();
                await BreadcrumbsService.SetGroupDetails(Group.Name);
                await MainTableService.InitializeAsync(GroupService.GetGruopMembersAsync, GroupService.GetGruopMembersCountAsync, ModalDialogService, StateHasChanged, nameof(GroupMembership.Employee.FullName), entityId: GroupId);
                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task LoadGroupAsync()
        {
            Group = await GroupService.GetGroupByIdAsync(GroupId);
            if (Group == null)
                throw new Exception("Group not found.");
            StateHasChanged();
        }

        private async Task OpenModalAddEmployeesAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddEmployee));
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
                builder.AddAttribute(2, "GroupId", GroupId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add Employees", body, ModalDialogSize.Large);
        }

        private async Task OpenModalDeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(RemoveEmployee));
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
                builder.AddAttribute(2, "GroupId", GroupId);
                builder.AddAttribute(3, "EmployeeId", MainTableService.SelectedEntity.EmployeeId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Employee", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.GroupDetails, async () =>
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