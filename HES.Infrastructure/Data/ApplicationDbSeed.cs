using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace HES.Infrastructure
{
    public class ApplicationDbSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbSeed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            await InitializeRoleAndAdmin();
            await InitializeDeviceAccessProfile();
            await InitializeIdentityProviderSettings();
        }

        private async Task InitializeRoleAndAdmin()
        {
            var roleResult = await _roleManager.RoleExistsAsync(ApplicationRoles.AdminRole);
            if (!roleResult)
            {
                // Create credentials
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";
                // Create role
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.AdminRole));
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.UserRole));
                // Create admin
                var admin = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                await _userManager.CreateAsync(admin, adminPassword);
                // Add admin to role
                await _userManager.AddToRoleAsync(admin, ApplicationRoles.AdminRole);
            }
        }

        private async Task InitializeDeviceAccessProfile()
        {
            var profile = await _context.DeviceAccessProfiles.FindAsync("default");

            if (profile == null)
            {
                await _context.DeviceAccessProfiles.AddAsync(new DeviceAccessProfile
                {
                    Id = "default",
                    Name = "Default",
                    CreatedAt = DateTime.UtcNow,
                    ButtonBonding = true,
                    ButtonConnection = false,
                    ButtonNewChannel = false,
                    PinBonding = false,
                    PinConnection = false,
                    PinNewChannel = false,
                    MasterKeyBonding = true,
                    MasterKeyConnection = false,
                    MasterKeyNewChannel = false,
                    PinExpiration = 86400,
                    PinLength = 4,
                    PinTryCount = 10,
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task InitializeIdentityProviderSettings()
        {
            var samlIdPEnabled = await _context.SamlIdentityProvider.FindAsync(SamlIdentityProvider.PrimaryKey);
            if (samlIdPEnabled == null)
            {
                await _context.SamlIdentityProvider.AddAsync(new SamlIdentityProvider
                {
                    Id = SamlIdentityProvider.PrimaryKey,
                    Enabled = false,
                    Url = "url"
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}