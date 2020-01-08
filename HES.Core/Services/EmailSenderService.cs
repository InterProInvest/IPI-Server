using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly SmtpClient client;
        private readonly string host;
        private readonly int port;
        private readonly bool enableSSL;
        private readonly string userName;
        private readonly string password;


        public EmailSenderService(IConfiguration config, IApplicationUserService applicationUserService)
        {
            host = config.GetValue<string>("EmailSender:Host");
            port = config.GetValue<int>("EmailSender:Port");
            enableSSL = config.GetValue<bool>("EmailSender:EnableSSL");
            userName = config.GetValue<string>("EmailSender:UserName");
            password = config.GetValue<string>("EmailSender:Password");

            client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
            };

            _applicationUserService = applicationUserService;
        }

        private async Task SendAsync(string email, string subject, string message)
        {
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
            string subject = "HES notification";
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
                                    your HES 
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