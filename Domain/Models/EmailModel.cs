using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace Domain.Models;

public class EmailModel
{
	public IEnumerable<EmailAddresses>? ToAddresses { get; set; }
	public IEnumerable<EmailAddresses>? CCAddresses { get; set; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; set; }

	[Required]
	public JsonObject Data { get; set; } = null!;

	[MaxLength(5)]
	public string Language { get; set; } = "en-GB";

	[Required]
	public string TemplateId { get; set; } = null!;
}

public class EmailAddresses
{
	public string? Name { get; set; }

	[EmailAddress]
	public string Email { get; set; } = null!;
}