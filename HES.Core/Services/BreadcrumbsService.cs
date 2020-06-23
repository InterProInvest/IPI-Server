using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class BreadcrumbsService : IBreadcrumbsService
    {
        public List<Breadcrumb> Breadcrumbs { get; set; }

        public Task GetBreadcrumbs(out List<Breadcrumb> items)
        {
            items = Breadcrumbs;
            Breadcrumbs = null;

            return Task.CompletedTask;
        }

        public Task SetDashboard()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Dashboard" }
            };

            return Task.CompletedTask;
        }

        public Task SetEmployees()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Employees" }
            };

            return Task.CompletedTask;
        }

        public Task SetEmployeeDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Employees", Content = "Employees" },
                new Breadcrumb () { Active = true, Content = name}
            };

            return Task.CompletedTask;
        }

        public Task SetHardwareVaults()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Hardware Vaults" }
            };

            return Task.CompletedTask;
        }

        public Task SetGroups()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Groups" }
            };

            return Task.CompletedTask;
        }

        public Task SetGroupDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Groups", Content = "Groups" },
                new Breadcrumb () { Active = true, Content = name}
            };

            return Task.CompletedTask;
        }

        public Task SetLicenseOrders()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "License Orders" }
            };

            return Task.CompletedTask;
        }

        public Task SetSharedAccounts()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Shared Accounts" }
            };

            return Task.CompletedTask;
        }

        public Task SetParameters()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Parameters" }
            };

            return Task.CompletedTask;
        }

        public Task SetWorkstations()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Workstations" }
            };

            return Task.CompletedTask;
        }

        public Task SetWorkstationDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Workstations", Content = "Workstations" },
                new Breadcrumb () { Active = true, Content = name}
            };

            return Task.CompletedTask;
        }
    }
}