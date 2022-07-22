using MimeKit;

namespace EmailApi.Models;

public class EmailModel
{
	public IEnumerable<EmailAddresses> ToAddresses { get; set; } = new List<EmailAddresses>();
	public string Subject { get; set; } = string.Empty;
	public string TextBody { get; set; } = string.Empty;
	public string HtmlBody { get; set; } = string.Empty;
}

public class EmailAddresses
{
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
}
