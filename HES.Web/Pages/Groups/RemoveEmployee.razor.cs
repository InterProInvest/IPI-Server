﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class RemoveEmployee : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<RemoveEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public string GroupId { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public GroupMembership GroupMembership { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupMembership = await GroupService.GetGroupMembershipAsync(EmployeeId, GroupId);

                if (GroupMembership == null)
                    throw new Exception("Group membership not found");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                await GroupService.RemoveEmployeeFromGroupAsync(GroupMembership.Id);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.GroupDetails);
                ToastService.ShowToast("Employee removed.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                await ModalDialogService.CloseAsync();
            }
        }
    }
}