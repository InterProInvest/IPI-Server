using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Web.SoftwareVault;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmailSenderService : IDisposable
    {
        Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status);
        Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults);
        Task SendActivateDataProtectionAsync();
        Task SendUserInvitationAsync(string email, string callbackUrl);
        Task SendUserResetPasswordAsync(string email, string callbackUrl);
        Task SendUserConfirmEmailAsync(string email, string callbackUrl);
        Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo);
        Task SendHardwareVaultActivationCodeAsync(Employee employee, string code);
    }
}