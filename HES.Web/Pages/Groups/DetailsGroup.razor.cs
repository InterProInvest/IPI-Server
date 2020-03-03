using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class DetailsGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<DetailsGroup> Logger { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Group Group { get; set; }
        public IList<GroupMembership> GroupMemberships { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadGroupMembershipsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await CreateBreadcrumbsAsync();
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

        private async Task CreateBreadcrumbsAsync()
        {
            var items = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link="/Groups", Content = "Groups" },
                new Breadcrumb () { Active = true, Content = "Details" }
            };
            await BreadcrumbsWrapper.BreadcrumbsComponent.ShowAsync(items);
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

            await MainWrapper.ModalDialogComponent.ShowAsync("Add employees", body);
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

            await MainWrapper.ModalDialogComponent.ShowAsync("Remove employee", body);
        }
    }
}