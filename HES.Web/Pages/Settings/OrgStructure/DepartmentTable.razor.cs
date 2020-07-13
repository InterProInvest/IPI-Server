using HES.Core.Entities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class DepartmentTable : ComponentBase
    {
        [Parameter] public RenderFragment CompanyBody { get; set; }
        [Parameter] public List<Department> Departments { get; set; }
        [Parameter] public Func<Department, Task> EditDepartmentDialog { get; set; }
        [Parameter] public Func<Department, Task> DeleteDepartmentDialog { get; set; }

        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;

        protected override void OnParametersSet()
        {
            if (IsSortedAscending)
            {
                Departments = Departments.OrderBy(x => x.Name).ToList();
            }
            else
            {
                Departments = Departments.OrderByDescending(x => x.Name).ToList();
            }
        }

        private string GetSortIcon()
        {
            if (IsSortedAscending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        private void SortTable()
        {
            IsSortedAscending = !IsSortedAscending;

            if (IsSortedAscending)
            {
                Departments = Departments.OrderBy(x => x.Name).ToList();
            }
            else
            {
                Departments = Departments.OrderByDescending(x => x.Name).ToList();
            }
        }
    }
}