using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class DetailsGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DetailsGroup> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Group Group { get; set; }
        public IList<GroupMembership> GroupMemberships { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadGroupMembershipsAsync();
                await BreadcrumbsService.SetGroupDetails(Group.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task LoadGroupMembershipsAsync()
        {
            Group = await GroupService.GetGroupByIdAsync(GroupId);
            if (Group == null)
            {
                NavigationManager.NavigateTo("/NotFound");
            }
            GroupMemberships = await GroupService.GetGruopMembersAsync(GroupId);
        }

        private async Task OpenModalAddEmployees()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddEmployee));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupMembershipsAsync));
                builder.AddAttribute(2, "GroupId", GroupId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add employees", body, ModalDialogSize.Large);
        }

        private async Task OpenModalRemoveEmployee(string employeeId)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(RemoveEmployee));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupMembershipsAsync));
                builder.AddAttribute(2, "GroupId", GroupId);
                builder.AddAttribute(3, "EmployeeId", employeeId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Remove employee", body);
        }
    }
}