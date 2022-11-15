using System.Text.Json.Nodes;
using MimeKit;

namespace Domain.Services.Email;

public interface IEmailService
{
	Task SendEmail(IEnumerable<MailboxAddress> to, string subject, string htmlBody, string textBody);
	
	/// <summary>
	/// Combines handlebars templates with data
	/// </summary>
	/// <param name="data"></param>
	/// <param name="templateHtmlBody"></param>
	/// <param name="templateSubject"></param>
	/// <exception cref="ArgumentException">Thrown on any combining template with data</exception>
	ConstructedEmail ConstructEmail(JsonObject data, string templateHtmlBody, string templateSubject);
}