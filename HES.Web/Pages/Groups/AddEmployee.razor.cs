using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class AddEmployee : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<AddEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Dictionary<Employee, bool> Employees { get; set; }

        private bool _notSelected { get; set; }
        private bool _isSelectedAll { get; set; }
        private bool _isBusy { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var employees = await GroupService.GetEmployeesSkipExistingOnesInGroupAsync(GroupId);
                Employees = employees.ToDictionary(k => k, v => false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        public async Task AddAsync()
        {
            try
            {
                if (!Employees.Any(x => x.Value == true))
                {
                    _notSelected = true;
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;
                var employeeIds = Employees.Where(x => x.Value).Select(x => x.Key.Id).ToList();

                await GroupService.AddEmployeesToGroupAsync(employeeIds, GroupId);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Employee added.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void OnRowSelected(Employee key)
        {
            Employees[key] = !Employees[key];
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            _isSelectedAll = !_isSelectedAll;
            foreach (var key in Employees.Keys.ToList())
                Employees[key] = _isSelectedAll;
        }
    }
}