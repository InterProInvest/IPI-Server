using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class EditGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<EditGroup> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string GroupId { get; set; }
        public Group Group { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Group = await GroupService.GetGroupByIdAsync(GroupId);

                if (Group == null)
                {
                    throw new Exception("Group not found");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        public async Task EditAsync()
        {
            try
            {
                await GroupService.EditGroupAsync(Group);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Group updated.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }
    }
}