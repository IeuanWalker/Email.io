using MailKit.Net.Smtp;
using MimeKit;

namespace App.Services.Email
{
    public class EmailService : IEmailService
    {
        public async Task SendEmail(IEnumerable<MailboxAddress> to, string subject, string htmlBody, string textBody)
        {
            string? mailHost = string.Empty;
            int mailPort = 0;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MailHostUrl")))
            {
                mailHost = Environment.GetEnvironmentVariable("MailHostUrl");
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MailPort")))
            {
                mailPort = Convert.ToInt32(Environment.GetEnvironmentVariable("MailPort"));
            }

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Test", "noreply@test.com"));
            foreach (MailboxAddress email in to)
            {
                message.To.Add(email);
            }
            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = textBody
            }.ToMessageBody();

            using SmtpClient mailClient = new SmtpClient();
            await mailClient.ConnectAsync(mailHost, mailPort, MailKit.Security.SecureSocketOptions.None);
            await mailClient.SendAsync(message);
            await mailClient.DisconnectAsync(true);
        }
    }
}