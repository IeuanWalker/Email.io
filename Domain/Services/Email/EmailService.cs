using System.Text.Json.Nodes;
using Database.Models;
using Database.Repositories.Email;
using Domain.Services.Handlebars;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Domain.Services.Email;

public class EmailService : IEmailService
{
	readonly IEmailRepository _emailRepository;
	readonly IHandleBarsService _handleBarsService;

	public EmailService(IEmailRepository emailRepository, IHandleBarsService handleBarsService)
	{
		_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
		_handleBarsService = handleBarsService ?? throw new ArgumentNullException(nameof(handleBarsService));
	}

	public async Task SendEmail(IEnumerable<MailboxAddress> toAddresses, IEnumerable<MailboxAddress>? ccAddresses, IEnumerable<MailboxAddress>? bccAddresses, string subject, string htmlContent, string plainTextContent, List<EmailAttachmentTbl>? attachments = null)
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

		MimeMessage message = new();
		message.From.Add(new MailboxAddress("Test", "noreply@test.com"));
		message.To.AddRange(toAddresses);
		if (ccAddresses?.Any() ?? false)
		{
			message.Cc.AddRange(ccAddresses);
		}
		if (bccAddresses?.Any() ?? false)
		{
			message.Bcc.AddRange(bccAddresses);
		}
		message.Subject = subject;

		var bodyBuilder = new BodyBuilder
		{
			HtmlBody = htmlContent,
			TextBody = plainTextContent
		};

		if (attachments?.Any() ?? false)
		{
			foreach (var attachment in attachments)
			{
				bodyBuilder.Attachments.Add(attachment.FileName, Convert.FromBase64String(attachment.Content), ContentType.Parse(attachment.ContentType));
			}
		}

		message.Body = bodyBuilder.ToMessageBody();

		using SmtpClient mailClient = new();
		await mailClient.ConnectAsync(mailHost, mailPort, SecureSocketOptions.None);
		await mailClient.SendAsync(message);
		await mailClient.DisconnectAsync(true);
	}

	// TODO: Use polly
	public async Task SendEmail(int emailId)
	{
		EmailTbl? email = await _emailRepository.Where(x => x.Id == emailId)
			.Include(x => x.ToAddresses)
			.Include(x => x.CCAddresses)
			.Include(x => x.BCCAddresses)
			.Include(x => x.Attachements)
			.FirstOrDefaultAsync();

		if (email is null || email.Sent is not null)
		{
			return;
		}

		await SendEmail(
			email.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.CCAddresses?.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.BCCAddresses?.Select(x => new MailboxAddress(x.Name, x.Email)),
			email.Subject,
			email.HtmlContent,
			email.PlainTextContent,
			email.Attachements?.ToList());

		email.Sent = DateTime.Now;

		_emailRepository.Update(email);
	}

	public ConstructedEmail ConstructEmail(JsonNode data, string subjectTemplate, string htmlTemplate, string? plainTextTemplate)
	{
		return new ConstructedEmail
		{
			Subject = _handleBarsService.Render(subjectTemplate, data),
			HtmlContent = _handleBarsService.Render(htmlTemplate, data),
			PlainTextContent = _handleBarsService.Render(plainTextTemplate, data),
		};
	}
}

public class ConstructedEmail
{
	public string Subject { get; set; } = string.Empty;
	public string HtmlContent { get; set; } = string.Empty;
	public string PlainTextContent { get; set; } = string.Empty;
}