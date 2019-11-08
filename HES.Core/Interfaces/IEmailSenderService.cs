using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}