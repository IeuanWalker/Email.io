using MimeKit;

namespace App.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(IEnumerable<MailboxAddress> to, string subject, string htmlBody, string textBody);
    }
}