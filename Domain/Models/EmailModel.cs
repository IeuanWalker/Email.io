using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.Models;

public class EmailModel
{
	public IEnumerable<EmailAddresses>? ToAddresses { get; set; }
	public IEnumerable<EmailAddresses>? CCAddresses { get; set; }
	public IEnumerable<EmailAddresses>? BCCAddresses { get; set; }

	[Required]
	public string Data { get; set; } = null!;

	[MaxLength(5)]
	public string Language { get; set; } = "en-GB";

	[Required]
	public string TemplateId { get; set; } = null!;

	public IEnumerable<IFormFile>? Attachments { get; set; }
}

public class EmailAddresses
{
	public string? Name { get; set; }

	[EmailAddress]
	public string Email { get; set; } = null!;
}