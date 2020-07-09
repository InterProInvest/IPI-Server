using HES.Core.Entities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class DepartmentTable : ComponentBase
    {
        [Parameter] public RenderFragment CompanyBody { get; set; }
        [Parameter] public List<Department> Departments { get; set; }

        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;

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
            if (IsSortedAscending)
            {
                Departments = Departments.OrderBy(x => x.Name).ToList();
            }
            else
            {
                Departments = Departments.OrderByDescending(x => x.Name).ToList();
            }

            IsSortedAscending = !IsSortedAscending;
        }
    }
}