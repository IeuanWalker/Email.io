using MimeKit;

namespace EmailApi.Models;

public class EmailModel
{
	public IEnumerable<MailboxAddress> ToAddresses { get; set; } = new List<MailboxAddress>();
	public string Subject { get; set; } = string.Empty;
	public string TextBody { get; set; } = string.Empty;
	public string HtmlBody { get; set; } = string.Empty;
}
