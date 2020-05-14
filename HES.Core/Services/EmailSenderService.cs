using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Hosting;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IHostingEnvironment _env;

        public EmailSenderService(IApplicationUserService applicationUserService,
                                  IAppSettingsService appSettingsService,
                                  IHostingEnvironment env)
        {
            _applicationUserService = applicationUserService;
            _appSettingsService = appSettingsService;
            _env = env;
        }

        private async Task SendAsync(string email, string subject, string message)
        {
            var settings = await _appSettingsService.GetEmailSettingsAsync();
            if (settings == null)
                throw new Exception("Email settings not set.");

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.UserName, settings.Password),
                EnableSsl = settings.EnableSSL
            };

            await client.SendMailAsync(new MailMessage(settings.UserName, email, subject, message) { IsBodyHtml = true });
        }

        private async Task SendAsync(MailMessage mailMessage, EmailSettings settings)
        {
            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.UserName, settings.Password),
                EnableSsl = settings.EnableSSL
            };

            await client.SendMailAsync(mailMessage);
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

        public async Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status)
        {
            var server = await _appSettingsService.GetServerSettingsAsync();
            string subject = server == null ? "HES notification" : $"({server.Name}) Notification";
            string hes = server == null ? "HES" : $"<a href='{server.Url}'>HES</a>";
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

        public async Task SendDeviceLicenseStatus(List<HardwareVault> devices)
        {
            var message = new StringBuilder();

            var valid = devices.Where(d => d.LicenseStatus == VaultLicenseStatus.Valid).OrderBy(d => d.Id).ToList();
            foreach (var item in valid)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var warning = devices.Where(d => d.LicenseStatus == VaultLicenseStatus.Warning).OrderBy(d => d.Id).ToList();
            foreach (var item in warning)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (90 days remainin)<br/>");
            }

            var critical = devices.Where(d => d.LicenseStatus == VaultLicenseStatus.Critical).OrderBy(d => d.Id).ToList();
            foreach (var item in critical)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (30 days remainin)<br/>");
            }

            var expired = devices.Where(d => d.LicenseStatus == VaultLicenseStatus.Expired).OrderBy(d => d.Id).ToList();
            foreach (var item in expired)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var server = await _appSettingsService.GetServerSettingsAsync();
            string subject = server == null ? "HES notification" : $"({server.Name}) Notification";
            string hes = server == null ? "HES" : $"<a href='{server.Url}'>HES</a>";
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

        public async Task SendAdminInvitationAsync(string email, string callbackUrl)
        {
            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var htmlMessage = GetTemplate("mail_admin-invitation");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Action required - Invitation to Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo)
        {
            if (employee.Email == null)
                throw new ArgumentNullException(nameof(employee.Email));

            var settings = await _appSettingsService.GetEmailSettingsAsync();
            if (settings == null)
                throw new Exception("Email settings not set.");

            var htmlMessage = GetTemplate("mail_software-vault-invitation");
            htmlMessage = htmlMessage.Replace("{{employeeName}}", employee.FirstName)
                .Replace("{{validTo}}", validTo.Date.ToShortDateString())
                .Replace("{{serverAddress}}", activation.ServerAddress)
                .Replace("{{activationId}}", activation.ActivationId)
                .Replace("{{activationCode}}", activation.ActivationCode.ToString());

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                                                         htmlMessage,
                                                         Encoding.UTF8,
                                                         MediaTypeNames.Text.Html);

            var code = $"{activation.ServerAddress}\n{activation.ActivationId}\n{activation.ActivationCode}";
            var qrCode = GetQRCode(code);

            string mediaType = MediaTypeNames.Image.Jpeg;
            LinkedResource img = new LinkedResource(qrCode, mediaType);
            img.ContentId = "QRCode";
            img.ContentType.MediaType = mediaType;
            img.TransferEncoding = TransferEncoding.Base64;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);

            htmlView.LinkedResources.Add(img);

            MailMessage mailMessage = new MailMessage(settings.UserName, employee.Email);
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Hideez Software Vault application";

            await SendAsync(mailMessage, settings);
        }

        public async Task SendHardwareVaultActivationCodeAsync(Employee employee, string code)
        {
            string subject = "Hideez Enterpise Server - Hardware vault";
            string html = $@"
                            <div style='font-family: Roboto;'>
                                <h1 style='color: #0E3059;'>Hideez Enterprise Server</h1>
                                <div style='margin-bottom: 15px; font-weight: 400; line-height: 1.5;font-size: 14px;'>
                                    Your hardware vault activation code: <b>{code}</b>
                                </div>                    
                            </div>
                           ";
            await SendAsync(employee.Email, subject, html);
        }

        private async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = await _appSettingsService.GetEmailSettingsAsync();

            if (settings == null)
                throw new Exception("Email settings not set.");

            return settings;
        }

        private async Task<ServerSettings> GetServerSettingsAsync()
        {
            var settings = await _appSettingsService.GetServerSettingsAsync();

            if (settings == null)
                throw new Exception("Server settings not set.");

            return settings;
        }

        private string GetTemplate(string name)
        {
            var path = Path.Combine(_env.WebRootPath, "templates", $"{name}.html");
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        private LinkedResource CreateImageResource(string name)
        {
            string mediaType = MediaTypeNames.Image.Jpeg;
            var img = new LinkedResource(Path.Combine(_env.WebRootPath, "templates", $"{name}.png"));
            img.ContentId = name;
            img.ContentType.MediaType = mediaType;
            img.TransferEncoding = TransferEncoding.Base64;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);
            return img;
        }

        private MemoryStream GetQRCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var qrCodeImageData = BitmapToBytes(qrCodeImage);
            return new MemoryStream(qrCodeImageData);
        }

        private byte[] BitmapToBytes(Bitmap img)
        {
            using MemoryStream stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}