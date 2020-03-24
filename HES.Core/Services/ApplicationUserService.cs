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

        public ApplicationUserService(IAsyncRepository<ApplicationUser> applicationUserRepository,
                                      UserManager<ApplicationUser> userManager)
        {
            _applicationUserRepository = applicationUserRepository;
            _userManager = userManager;
        }

        public IQueryable<ApplicationUser> Query()
        {
            return _applicationUserRepository.Query();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _applicationUserRepository.GetByIdAsync(id);
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

        public async Task<IList<ApplicationUser>> GetAllAsync()
        {
            return await _applicationUserRepository.Query().ToListAsync();
        }

        public async Task<IList<ApplicationUser>> GetAdministratorsAsync()
        {
            return await _userManager.GetUsersInRoleAsync("Administrator");
        }
    }
}