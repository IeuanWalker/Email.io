using System.Text.Json.Nodes;
using MimeKit;

namespace Domain.Services.Email;

public interface IEmailService
{
	Task SendEmail(IEnumerable<MailboxAddress> toAddresses, IEnumerable<MailboxAddress>? ccAddresses, IEnumerable<MailboxAddress>? bccAddresses, string subject, string htmlContent, string plainTextContent);
	Task SendEmail(Guid emailId);

	/// <summary>
	/// Combines handlebars templates with data
	/// </summary>
	/// <param name="data"></param>
	/// <param name="subjectTemplate"></param>
	/// <param name="htmlTemplate"></param>
	/// <param name="plainTextTemplate"></param>
	/// <exception cref="ArgumentException">Thrown on any combining template with data</exception>
	ConstructedEmail ConstructEmail(JsonObject data, string subjectTemplate, string htmlTemplate, string? plainTextTemplate);
}