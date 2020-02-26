using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Group;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class ManageEmployees : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<ManageEmployees> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string GroupId { get; set; }
        List<GroupEmployee> GroupEmployees { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupEmployees = await GroupService.GetMappedGroupEmployeesAsync(GroupId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await GroupService.ManageEmployeesAsync(GroupEmployees, GroupId);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Members updated.", ToastLevel.Success);
                await MainWrapper.ModalDialogComponent.CloseAsync();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        public void RowSelected(string employeeId)
        {
            var member = GroupEmployees.FirstOrDefault(x => x.Employee.Id == employeeId);
            member.InGroup = !member.InGroup;
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            GroupEmployees.ForEach(x => x.InGroup = (bool)args.Value);
        }
    }
}
