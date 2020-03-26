using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
        private readonly string host;
        private readonly int port;
        private readonly bool enableSSL;
        private readonly string userName;
        private readonly string password;

        public EmailSenderService(IConfiguration config,
                                    IApplicationUserService applicationUserService,
                                    IAppSettingsService appSettingsService,
                                    IHostingEnvironment env)
        {
            _applicationUserService = applicationUserService;
            _appSettingsService = appSettingsService;
            _env = env;

            host = config.GetValue<string>("EmailSender:Host");
            port = config.GetValue<int>("EmailSender:Port");
            enableSSL = config.GetValue<bool>("EmailSender:EnableSSL");
            userName = config.GetValue<string>("EmailSender:UserName");
            password = config.GetValue<string>("EmailSender:Password");
        }

        private async Task SendAsync(string email, string subject, string message)
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
            };

            await client.SendMailAsync(new MailMessage(userName, email, subject, message) { IsBodyHtml = true });
        }

        private async Task SendAsync(MailMessage mailMessage)
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
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

        public async Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo)
        {
            var htmlMessage = GetTemplate("software-vault-invitation");
            htmlMessage = htmlMessage.Replace("{{employeeName}}", employee.FirstName)
                .Replace("{{validTo}}", validTo.Date.ToString())
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

            MailMessage mailMessage = new MailMessage(userName, employee.Email);
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Hideez Software Vault application";

            await SendAsync(mailMessage);
        }

        private string GetTemplate(string name)
        {
            var path = Path.Combine(_env.WebRootPath, "templates", $"{name}.html");
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
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