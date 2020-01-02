using HES.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private string host;
        private int port;
        private bool enableSSL;
        private string userName;
        private string password;

        public EmailSenderService(IConfiguration config)
        {
            host = config.GetValue<string>("EmailSender:Host");
            port = config.GetValue<int>("EmailSender:Port");
            enableSSL = config.GetValue<bool>("EmailSender:EnableSSL");
            userName = config.GetValue<string>("EmailSender:UserName");
            password = config.GetValue<string>("EmailSender:Password");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
            };
            return client.SendMailAsync(new MailMessage(userName, email, subject, CreateTamplate(htmlMessage)) { IsBodyHtml = true });
        }

        private string CreateTamplate(string text)
        {
            string html = @"<div style='font-family: Roboto;'>
                                  <h1 style='color: #0E3059;'>Hideez Enterprise Server</h1>
                                 <div style='margin-bottom: 15px; font-weight: 400; line-height: 1.5;font-size: 14px;'>_text_</div>                    
                           </div>";
            return html.Replace("_text_", text);
        }
    }
}