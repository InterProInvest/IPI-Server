using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class BreadcrumbsService : IBreadcrumbsService
    {
        public event Func<List<Breadcrumb>, Task> OnSet;
        public List<Breadcrumb> Breadcrumbs { get; set; }

        public async Task SetDataProtection()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Data Protection" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetDashboard()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Dashboard" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetAdministrators()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Administrators" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetEmployees()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Employees" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetTemplates()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Templates" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetEmployeeDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Employees", Content = "Employees" },
                new Breadcrumb () { Active = true, Content = name}
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaults()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Hardware Vaults" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetGroups()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Groups" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetGroupDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Groups", Content = "Groups" },
                new Breadcrumb () { Active = true, Content = name}
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetLicenseOrders()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "License Orders" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaultProfiles()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Hardware Vault Access Profiles" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetSharedAccounts()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Shared Accounts" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationEvents()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Workstation Events" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationSessions()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Workstation Sessions" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetAuditSummaries()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Summaries" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetParameters()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Parameters" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetOrgStructure()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "OrgStructure" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstations()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Workstations" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstationDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Workstations", Content = "Workstations" },
                new Breadcrumb () { Active = true, Content = name}
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetTwoFactorAuthentication()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Two Factor Authentication" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetShowRecoveryCodes()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Show Recovery Codes" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetResetAuthenticator()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Reset Authenticator" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetProfile()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Profile" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetPersonalData()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Personal Data" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetGenerateRecoveryCodes()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Generate Recovery Codes" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetEnableAuthenticator()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Enable Authenticator" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }

        public async Task SetDisable2fa()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Disable 2FA" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }
        
        public async Task SetDeletePersonalData()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Delete Personal Data" }
            };

            await OnSet?.Invoke(Breadcrumbs);
        }
    }
}