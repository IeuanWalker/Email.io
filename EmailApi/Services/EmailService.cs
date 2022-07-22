using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using EmailApi.Models;

namespace EmailApi.Services;

public class EmailService : IEmailService
{
	readonly IConfiguration _config;

	public EmailService(IConfiguration config)
	{
		_config = config;
	}
	public void SendEmail(EmailModel request)
	{
		var email = new MimeMessage();

		//AddEmailAddresses(email, request.ToAddresses);

		email.To.Add(MailboxAddress.Parse("donny.schmitt54@ethereal.email"));
		email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:Username").Value));
		email.Subject = "Test Email Subject";
		email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = request.Body };

		try
		{
			using var smtp = new SmtpClient();
			smtp.Connect(_config.GetSection("Email:Host").Value, 587, SecureSocketOptions.StartTls);
			smtp.Authenticate(_config.GetSection("Email:Username").Value, _config.GetSection("Email:Password").Value);
			smtp.Send(email);
			smtp.Disconnect(true);
		}
		catch (Exception)
		{
			throw;
		}
	}

	void AddEmailAddresses(MimeMessage email, IEnumerable<string> toAddresses)
	{
		//TODO Null check?
		foreach (string address in toAddresses)
		{
			email.To.Add(MailboxAddress.Parse(address));
		}
	}
}

