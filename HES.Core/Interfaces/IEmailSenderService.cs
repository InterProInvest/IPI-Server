using HES.Core.Enums;
using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendLicenseChangedAsync(DateTime createdAt, OrderStatus status);
        Task SendActivateDataProtectionAsync();
    }
}