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
    public partial class CreateGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<CreateGroup> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Group Group = new Group();

        public NameUsage NameUsageMessage;

        private async Task CreateAsync()
        {
            try
            {                
                var isExist = await GroupService.CheckExistsGroupNameAsync(Group.Name);

                if (isExist)
                {
                    NameUsageMessage.DisplayError("Name", "This name is already in use.");
                    return;
                }

                await GroupService.CreateGroupAsync(Group);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Group created.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}