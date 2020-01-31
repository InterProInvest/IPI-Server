using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _config;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IAppSettingsService _appSettingsService;

        public EmailSenderService(IConfiguration config, IApplicationUserService applicationUserService, IAppSettingsService appSettingsService)
        {
            _config = config;
            _applicationUserService = applicationUserService;
            _appSettingsService = appSettingsService;
        }

        private async Task SendAsync(string email, string subject, string message)
        {
            var host = _config.GetValue<string>("EmailSender:Host");
            var port = _config.GetValue<int>("EmailSender:Port");
            var enableSSL = _config.GetValue<bool>("EmailSender:EnableSSL");
            var userName = _config.GetValue<string>("EmailSender:UserName");
            var password = _config.GetValue<string>("EmailSender:Password");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
            };

            await client.SendMailAsync(new MailMessage(userName, email, subject, message) { IsBodyHtml = true });
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string html = $@"
                            <div style='font-family: Roboto;'>
                                <h1 style='color: #0E3059;'>Hideez Enterprise Server</h1>
                                <div style='margin-bottom: 15px; font-weight: 400; line-height: 1.5;font-size: 14px;'>{htmlMessage}</div>                    
                            </div>
                           ";

            await SendAsync(email, subject, html);
        }

        public async Task SendLicenseChangedAsync(DateTime createdAt, OrderStatus status)
        {
            var server = await _appSettingsService.GetServerSettingsAsync();
            string subject = server.Name == null ? "HES notification" : $"({server.Name}) Notification";
            string hes = server.Url == null ? "HES" : $"<a href='{server.Url}'>HES</a>";
            string html = $@"
                           <div style='font-family: Roboto;'>                       
                                <div style='line-height: 1.5;font-size: 14px;'>Dear Admin,</div>
                                <br/>
                                <div style='font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    The status of your Hideez License order of {createdAt} has been changed to {status}
                                </div>    
                                <br/>
                                <div style='font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    Sincerely,<br/>
                                    your {hes} 
                                </div>  
                           </div>
                          ";

            var admins = await _applicationUserService.GetAdministratorsAsync();

            foreach (var admin in admins)
            {
                await SendAsync(admin.Email, subject, html);
            }
        }

        public async Task SendDeviceLicenseStatus(List<Device> devices)
        {
            var message = new StringBuilder();

            var valid = devices.Where(d => d.LicenseStatus == LicenseStatus.Valid).OrderBy(d => d.Id).ToList();
            foreach (var item in valid)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var warning = devices.Where(d => d.LicenseStatus == LicenseStatus.Warning).OrderBy(d => d.Id).ToList();
            foreach (var item in warning)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (90 days remainin)<br/>");
            }

            var critical = devices.Where(d => d.LicenseStatus == LicenseStatus.Critical).OrderBy(d => d.Id).ToList();
            foreach (var item in critical)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (30 days remainin)<br/>");
            }

            var expired = devices.Where(d => d.LicenseStatus == LicenseStatus.Expired).OrderBy(d => d.Id).ToList();
            foreach (var item in expired)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var server = await _appSettingsService.GetServerSettingsAsync();
            string subject = server.Name == null ? "HES notification" : $"({server.Name}) Notification";
            string hes = server.Url == null ? "HES" : $"<a href='{server.Url}'>HES</a>";
            string html = $@"
                           <div style='font-family: Roboto;'>                       
                                <div style='line-height: 1.5;font-size: 14px;'>Dear Admin,</div>
                                <br/>
                                <div style='font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    Some of your Hideez Key devices have changes in their license status. Please review the list below and take action if required.
                                    <br/>
                                    <br/>
                                    {message}
                                </div>    
                                <br/>
                                <div style='font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    Sincerely,<br/>
                                    your {hes} 
                                </div>  
                           </div>
                          ";

            var admins = await _applicationUserService.GetAdministratorsAsync();

            foreach (var admin in admins)
            {
                await SendAsync(admin.Email, subject, html);
            }
        }

        public async Task SendActivateDataProtectionAsync()
        {
            string subject = "Hideez Enterpise Server - Activate Data Protection";
            string html = $@"
                            <div style='font-family: Roboto;'>
                                <h1 style='color: #0E3059;'>Hideez Enterprise Server</h1>
                                <div style='margin-bottom: 15px; font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    The server has been restarted, you need to activate the data protection, please go to the server.
                                </div>                    
                            </div>
                           ";

            var admins = await _applicationUserService.GetAdministratorsAsync();

            foreach (var admin in admins)
            {
                await SendAsync(admin.Email, subject, html);
            }
        }
    }
}