﻿using HES.Core.Models.Web.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<LicensingSettings> GetLicensingSettingsAsync();
        Task SetLicensingSettingsAsync(LicensingSettings licensing);
        Task<EmailSettings> GetEmailSettingsAsync();
        Task SetEmailSettingsAsync(EmailSettings email);
        Task<ServerSettings> GetServerSettingsAsync();
        Task SetServerSettingsAsync(ServerSettings server);
        Task<LdapSettings> GetLdapSettingsAsync();
        Task SetLdapSettingsAsync(LdapSettings ldapSettings);
    }
}