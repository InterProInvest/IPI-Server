﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace HES.Infrastructure.Data
{
    public static class InitializationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }

            return host;
        }

        public static IHost SeedDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                InitializeRoleAndAdmin(scope).Wait();
                InitializeDeviceAccessProfile(scope).Wait();
                InitializeDataProtection(scope).Wait();
            }
         
            return host;
        }

        private static async Task InitializeRoleAndAdmin(IServiceScope scope)
        {
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roleResult = await roleManager.RoleExistsAsync(ApplicationRoles.AdminRole);
            if (!roleResult)
            {
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";

                // Create roles
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.AdminRole));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.UserRole));
                // Create admin
                var admin = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                await userManager.CreateAsync(admin, adminPassword);
                // Add admin to role
                await userManager.AddToRoleAsync(admin, ApplicationRoles.AdminRole);
            }
        }

        private static async Task InitializeDeviceAccessProfile(IServiceScope scope)
        {
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var profile = await context.DeviceAccessProfiles.FindAsync("default");

            if (profile == null)
            {
                await context.DeviceAccessProfiles.AddAsync(new DeviceAccessProfile
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

                await context.SaveChangesAsync();
            }
        }

        private static async Task InitializeDataProtection(IServiceScope scope)
        {
            var dataProtection = scope.ServiceProvider.GetRequiredService<IDataProtectionService>();
            await dataProtection.InitializeAsync();
        }
    }
}