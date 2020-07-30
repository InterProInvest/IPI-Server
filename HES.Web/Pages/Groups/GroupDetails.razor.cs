using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Groups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupDetails : ComponentBase
    {
        [Inject] public IMainTableService<GroupMembership, GroupMembershipFilter> MainTableService { get; set; }
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GroupDetails> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string GroupId { get; set; }

        private HubConnection hubConnection;

        public Group Group { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Group = await GroupService.GetGroupByIdAsync(GroupId);
                if (Group == null)
                    NavigationManager.NavigateTo("/NotFound");

                await MainTableService.InitializeAsync(GroupService.GetGruopMembersAsync, GroupService.GetGruopMembersCountAsync, StateHasChanged, nameof(GroupMembership.Employee.FullName), entityId: GroupId);
                await BreadcrumbsService.SetGroupDetails(Group.Name);
                await InitializeHubAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
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
            _ = hubConnection.DisposeAsync();
        }
    }
}