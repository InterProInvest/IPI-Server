using HES.Core.Entities;
using HES.Core.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HES.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Workstation> Workstations { get; set; }
        public DbSet<SharedAccount> SharedAccounts { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceAccount> DeviceAccounts { get; set; }
        public DbSet<DeviceTask> DeviceTasks { get; set; }
        public DbSet<ProximityDevice> ProximityDevices { get; set; }
        public DbSet<WorkstationEvent> WorkstationEvents { get; set; }
        public DbSet<WorkstationSession> WorkstationSessions { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<DeviceAccessProfile> DeviceAccessProfiles { get; set; }
        public DbSet<SamlIdentityProvider> SamlIdentityProvider { get; set; }
        public DbSet<DataProtection> DataProtection { get; set; }

        public DbQuery<SummaryByDayAndEmployee> SummaryByDayAndEmployee { get; set; }
        public DbQuery<SummaryByEmployees> SummaryByEmployees { get; set; }
        public DbQuery<SummaryByDepartments> SummaryByDepartments { get; set; }
        public DbQuery<SummaryByWorkstations> SummaryByWorkstations { get; set; }
    }
}