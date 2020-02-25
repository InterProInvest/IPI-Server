using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<GroupsPage> Logger { get; set; }
        public IList<Group> Groups { get; set; }
        public string CurrentGroupId { get; set; }
        public ElementReference CurrentRow { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadGroupsAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
            }
            await CreateBreadcrumbsAsync();
        }

        public async Task LoadGroupsAsync()
        {
            Groups = await GroupService.GetGroupsAsync();
            CurrentGroupId = null;
        }

        public async Task CreateBreadcrumbsAsync()
        {
            var items = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = "active", Content = "Groups" }
            };
            await BreadcrumbsWrapper.BreadcrumbsComponent.ShowAsync(items);
        }

        public void RowSelected(string groupId)
        {
            CurrentGroupId = groupId != CurrentGroupId ? groupId : null;
        }

        public async Task OpenModalGreateGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Create group", body);
        }

        public async Task OpenModalEditGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Edit group", body);
        }

        public async Task OpenModalDeleteGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Delete group", body);
        }
    }
}
