using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IAsyncRepository<ApplicationUser> _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

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

        public async Task DetachUserAsync(ApplicationUser user)
        {
            await _applicationUserRepository.DetachedAsync(user);
        }

        public async Task DetachUsersAsync(List<ApplicationUser> users)
        {
            foreach (var item in users)
            {
                await DetachUserAsync(item);
            }
        }

        

        public async Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            var query = _applicationUserRepository.Query();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(ApplicationUser.Email):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                    break;
                case nameof(ApplicationUser.PhoneNumber):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PhoneNumber) : query.OrderByDescending(x => x.PhoneNumber);
                    break;
                case nameof(ApplicationUser.EmailConfirmed):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.EmailConfirmed) : query.OrderByDescending(x => x.EmailConfirmed);
                    break;
                case nameof(ApplicationUser.TwoFactorEnabled):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.TwoFactorEnabled) : query.OrderByDescending(x => x.TwoFactorEnabled);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).ToListAsync();
        }
        public async Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            var query = _applicationUserRepository.Query();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<string> InviteAdministratorAsync(string email, string domain)
        {
            var userExist = await _userManager.FindByEmailAsync(email);

            if (userExist != null)
                throw new Exception($"User named {email} already exists");

            var user = new ApplicationUser { UserName = email, Email = email };
            var password = Guid.NewGuid().ToString();

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Administrator");

            if (!roleResult.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{domain}Identity/Account/Invite?code={WebUtility.UrlEncode(code)}&Email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }


        public async Task<string> GetCallBackUrl(string email, string domain)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if(user == null)
                throw new Exception($"User not found.");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{domain}Identity/Account/Invite?code={WebUtility.UrlEncode(code)}&Email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _applicationUserRepository.GetByIdAsync(id);
        }

        public async Task<ApplicationUser> DeleteUserAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var applicationUser = await _applicationUserRepository.GetByIdAsync(id);

            if (applicationUser == null)
                throw new Exception("User not found");


            //var currentUserId = _userManager.GetUserId(User);
            //if (id == currentUserId)
            //    await _signInManager.SignOutAsync();

            return await _applicationUserRepository.DeleteAsync(applicationUser);
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