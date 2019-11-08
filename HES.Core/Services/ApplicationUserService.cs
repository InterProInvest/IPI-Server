using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IAsyncRepository<ApplicationUser> _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;

        public ApplicationUserService(IAsyncRepository<ApplicationUser> applicationUserRepository,
                                      UserManager<ApplicationUser> userManager,
                                      IEmailSenderService emailSender)
        {
            _applicationUserRepository = applicationUserRepository;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IQueryable<ApplicationUser> Query()
        {
            return _applicationUserRepository.Query();
        }

        public async Task<ApplicationUser> GetByIdAsync(dynamic id)
        {
            return await _applicationUserRepository.GetByIdAsync(id);
        }

        public async Task<IList<ApplicationUser>> GetAllAsync()
        {
            return await _applicationUserRepository.Query().ToListAsync();
        }

        public async Task<IList<ApplicationUser>> GetOnlyAdministrators()
        {
            var administrators = new List<ApplicationUser>();

            var users = await _applicationUserRepository.Query().ToListAsync();

            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
                if (isAdmin)
                {
                    administrators.Add(user);
                }
            }
            return administrators;
        }

        public async Task DeleteUserAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var applicationUser = await _applicationUserRepository.GetByIdAsync(id);

            if (applicationUser != null)
            {
                await _applicationUserRepository.DeleteAsync(applicationUser);
            }
        }

        public async Task SendEmailDataProtectionNotify()
        {
            var administrators = await GetOnlyAdministrators();

            foreach (var admin in administrators)
            {
                await _emailSender.SendEmailAsync(admin.Email, "Hideez Enterprise Server", "Need to activate data protection");
            }
        }
    }
}
