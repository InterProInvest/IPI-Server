using HES.Core.Interfaces;
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


        public EmailSenderService(string host, int port, bool enableSSL, string userName, string password)
        {
            this.host = host;
            this.port = port;
            this.enableSSL = enableSSL;
            this.userName = userName;
            this.password = password;
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
            string html = @"<div style='margin: 5px; height: 300px;width: 500px;background-color: #F7F8FC;border-radius: 10px;box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); font-family: Roboto;'>
                <div style='padding: 25px;'>
                    <h1 style='color: #0E3059;'>Hideez Enterprise Server</h1>
                </div>
                <div style='padding: 0px 25px;'>
                    <div style='margin-bottom: 15px; font-weight: 400; line-height: 1.5;font-size: 14px;'>_text_</div>                    
                </div>
            </div>";

            return html.Replace("_text_", text);
        }

    }
}